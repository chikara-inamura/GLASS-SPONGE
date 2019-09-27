using System;
using System.Collections;
using System.Collections.Generic;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;



/// <summary>
/// This class will be instantiated on demand by the Script component.
/// </summary>
public class Script_Instance : GH_ScriptInstance
{
#region Utility functions
  /// <summary>Print a String to the [Out] Parameter of the Script component.</summary>
  /// <param name="text">String to print.</param>
  private void Print(string text) { /* Implementation hidden. */ }
  /// <summary>Print a formatted String to the [Out] Parameter of the Script component.</summary>
  /// <param name="format">String format.</param>
  /// <param name="args">Formatting parameters.</param>
  private void Print(string format, params object[] args) { /* Implementation hidden. */ }
  /// <summary>Print useful information about an object instance to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj) { /* Implementation hidden. */ }
  /// <summary>Print the signatures of all the overloads of a specific method to the [Out] Parameter of the Script component. </summary>
  /// <param name="obj">Object instance to parse.</param>
  private void Reflect(object obj, string method_name) { /* Implementation hidden. */ }
#endregion

#region Members
  /// <summary>Gets the current Rhino document.</summary>
  private readonly RhinoDoc RhinoDocument;
  /// <summary>Gets the Grasshopper document that owns this script.</summary>
  private readonly GH_Document GrasshopperDocument;
  /// <summary>Gets the Grasshopper script component that owns this script.</summary>
  private readonly IGH_Component Component;
  /// <summary>
  /// Gets the current iteration count. The first call to RunScript() is associated with Iteration==0.
  /// Any subsequent call within the same solution will increment the Iteration count.
  /// </summary>
  private readonly int Iteration;
#endregion

  /// <summary>
  /// This procedure contains the user code. Input parameters are provided as regular arguments,
  /// Output parameters as ref arguments. You don't have to assign output parameters,
  /// they will have a default value.
  /// </summary>
  private void RunScript(List<string> X, ref object LBC, ref object MBC, ref object LHL, ref object LHR, ref object LSL, ref object LSR, ref object MHL, ref object MHR, ref object PHL, ref object PHR)
  {

    // DEBUG PARAMETER
    List<int> lsIDX = new List<int>();
    lsIDX.Add(1); // idX = 0;
    lsIDX.Add(1); // idX = 1;
    lsIDX.Add(1); // idX = 2;
    lsIDX.Add(1); // idX = 3;
    lsIDX.Add(1); // idX = 4;
    lsIDX.Add(1); // idX = 5;
    lsIDX.Add(1); // idX = 6;
    lsIDX.Add(1); // idX = 7;
    lsIDX.Add(1); // idX = 8;
    lsIDX.Add(1); // idX = 9;
    lsIDX.Add(1); // idX = 10;
    lsIDX.Add(1); // idX = 11;
    lsIDX.Add(1); // idX = 12;
    lsIDX.Add(1); // idX = 13;
    lsIDX.Add(1); // idX = 14;
    lsIDX.Add(1); // idX = 15;

    // INPUT PARAMETERS
    bool bDiag1 = false;
    bool bDiag2 = true;
    bool bDiagFull = false;
    bool bRidge = true;
    bool bOpening = true;
    bool bCap = true;
    bool bThick = true;
    int iNumH = 32;
    int iNumV = 16;
    int iNumPerH = 1;
    double dSpace = 1.0;
    double dThick = 0.2 * dSpace;

    // INPUT MATRIX
    int i = 0;
    int j = 0;
    int iX = X.Count;
    double[][] dX = new double [iX][];
    for(i = 0; i < iX; i++)
    {
      string sX = X[i];
      string[] aSX = sX.Split(',');
      dX[i] = new double [aSX.Length];
      for (j = 0; j < aSX.Length; j++)
      {
        double dSX = Convert.ToDouble(aSX[j]);
        dX[i][j] = dSX;
      }
    }

    // INTERNAL PARAMETERS
    double dR1 = dSpace * (double) iNumPerH / (2.0 * Math.Sin(Math.PI / (1.0 * (double) iNumH)));
    double dK = 6.0 / 42.5; // coefficient ratio between diameter of the sponge and the height of the ridge
    double dD = dK * 2 * dR1; // space between Polygonal and Helix
    double dR2 = dR1 + dD;
    double dAngIntDeg = 180 * ((double) iNumH - 2.0) / (1.0 * (double) iNumH); // INTERN ANGLE
    double dAngExtDeg = 360.0 / (1.0 * (double) iNumH); // EXTERN ANGLE
    double dAngDeg = 90 + 0.5 * dAngExtDeg;
    double dAngStpRad = dAngExtDeg * Math.PI / 180.0 / 2.0;
    List<Point3d> lsPT_S = new List<Point3d>(); // list for points coordinates of Supports
    List<Point3d> lsPT_H = new List<Point3d>(); // list for points coordinates of Helices

    // DATA VALIDATION FOR INPUT PARATMETERS
    if (iNumH < 3){ return;}
    if (iNumH % 4 == 1){ return; }
    if (iNumPerH < 1) { return; }
    if (iNumV < 2){ return; }

    // UNICEL CYLINDER
    // 1 - CREATING THE MAIN UNICEL
    List<Line> lsL_U1 = new List<Line>();
    lsL_U1.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 0.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 0.0 * dSpace)));
    lsL_U1.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 2.0 * dSpace), new Point3d(0.0 * dSpace, 0.0, 0.0 * dSpace)));
    lsL_U1.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 1.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 1.0 * dSpace)));
    if (bDiag1 == true)
    {
      lsL_U1.Add(new Line(new Point3d(1.0 * dSpace, 0.0, 1.0 * dSpace), new Point3d(0.0 * dSpace, 0.0, 2.0 * dSpace)));
      lsL_U1.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 1.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 2.0 * dSpace)));
    }
    if (bDiagFull == true)
    {
      lsL_U1.Add(new Line(new Point3d(1.0 * dSpace, 0.0, 1.0 * dSpace), new Point3d(0.0 * dSpace, 0.0, 2.0 * dSpace)));
      lsL_U1.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 1.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 2.0 * dSpace)));
      lsL_U1.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 0.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 1.0 * dSpace)));
      lsL_U1.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 1.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 0.0 * dSpace)));
    }
    if (bDiag2 == true)
    {
      double x1 = Math.Sqrt(2.0) * dSpace / (Math.Sqrt(2.0) * 2.0);
      double x2 = Math.Sqrt(1.0) * dSpace / (Math.Sqrt(2.0) + 2.0);
      lsL_U1.Add(new Line(new Point3d(1.0 * dSpace - 1.0 * x2, 0.0, 0.0 * dSpace + 0.0 * x2), new Point3d(1.0 * dSpace + 0.0, 0.0, 0.0 * dSpace + x2)));
      lsL_U1.Add(new Line(new Point3d(1.0 * dSpace + 0.0 * x2, 0.0, 1.0 * dSpace - 1.0 * x2), new Point3d(0.0 * dSpace + 0.0, 0.0, 2.0 * dSpace - x2)));
      lsL_U1.Add(new Line(new Point3d(1.0 * dSpace + 0.0 * x2, 0.0, 1.0 * dSpace + 1.0 * x2), new Point3d(0.0 * dSpace + x2, 0.0, 2.0 * dSpace + 0.0)));
      lsL_U1.Add(new Line(new Point3d(0.0 * dSpace + 0.0 * x2, 0.0, 1.0 * dSpace - 1.0 * x2), new Point3d(1.0 * dSpace + 0.0, 0.0, 2.0 * dSpace - x2)));
      lsL_U1.Add(new Line(new Point3d(0.0 * dSpace + 0.0 * x2, 0.0, 1.0 * dSpace + 1.0 * x2), new Point3d(1.0 * dSpace - x2, 0.0, 2.0 * dSpace + 0.0)));
      lsL_U1.Add(new Line(new Point3d(0.0 * dSpace + 0.0 * x2, 0.0, 0.0 * dSpace + 1.0 * x2), new Point3d(0.0 * dSpace + x2, 0.0, 0.0 * dSpace + 0.0)));
    }
    // 2 - CREATING SECOND PART OF UNIT-CELL
    List<Line> lsL_U2 = new List<Line>();
    lsL_U2.Add(new Line(new Point3d(0.0, 0.0, 0.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 0.0 * dSpace)));
    lsL_U2.Add(new Line(new Point3d(0.0, 0.0, 2.0 * dSpace), new Point3d(0.0 * dSpace, 0.0, 0.0 * dSpace)));
    lsL_U2.Add(new Line(new Point3d(0.0, 0.0, 1.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 1.0 * dSpace)));
    if (bDiag1 == true)
    {
      lsL_U2.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 0.0), new Point3d(1.0 * dSpace, 0.0, 1.0 * dSpace)));
      lsL_U2.Add(new Line(new Point3d(1.0 * dSpace, 0.0, 0.0), new Point3d(0.0 * dSpace, 0.0, 1.0 * dSpace)));
    }
    if (bDiagFull == true)
    {
      lsL_U2.Add(new Line(new Point3d(1.0 * dSpace, 0.0, 1.0 * dSpace), new Point3d(0.0 * dSpace, 0.0, 2.0 * dSpace)));
      lsL_U2.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 1.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 2.0 * dSpace)));
      lsL_U2.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 0.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 1.0 * dSpace)));
      lsL_U2.Add(new Line(new Point3d(0.0 * dSpace, 0.0, 1.0 * dSpace), new Point3d(1.0 * dSpace, 0.0, 0.0 * dSpace)));
    }
    if (bDiag2 == true)
    {
      double x2 = dSpace / (Math.Sqrt(2.0) + 2.0);
      lsL_U2.Add(new Line(new Point3d(0.0 * dSpace + 0.0, 0.0, 0.0 * dSpace + x2), new Point3d(1.0 * dSpace + 0.0, 0.0, 1.0 * dSpace + x2)));
      lsL_U2.Add(new Line(new Point3d(0.0 * dSpace + x2, 0.0, 0.0 * dSpace + 0.0), new Point3d(1.0 * dSpace + 0.0, 0.0, 1.0 * dSpace - x2)));
      lsL_U2.Add(new Line(new Point3d(1.0 * dSpace - x2, 0.0, 0.0 * dSpace + 0.0), new Point3d(0.0 * dSpace + 0.0, 0.0, 1.0 * dSpace - x2)));
      lsL_U2.Add(new Line(new Point3d(1.0 * dSpace + 0.0, 0.0, 0.0 * dSpace + x2), new Point3d(0.0 * dSpace + 0.0, 0.0, 1.0 * dSpace + x2)));
      lsL_U2.Add(new Line(new Point3d(0.0 * dSpace + 0.0, 0.0, 2.0 * dSpace - x2), new Point3d(0.0 * dSpace + x2, 0.0, 2.0 * dSpace + 0.0)));
      lsL_U2.Add(new Line(new Point3d(1.0 * dSpace - x2, 0.0, 2.0 * dSpace + 0.0), new Point3d(1.0 * dSpace + 0.0, 0.0, 2.0 * dSpace - x2)));
    }
    // BEGINNING CONSTRUCTION OF THE ASSEMBLY OF CYLINDER   #It adds the two unicels in the assembly
    List<Line> lsL_AU1 = new List<Line>();
    lsL_AU1.AddRange(lsL_U1);
    List<Line> lsL_AU2 = new List<Line>();
    lsL_AU2.AddRange(lsL_U2);
    // CONSTRUCTION OF ASSEMBLY FOR CASES WHERE THERE ARE ODD NUMBERS OF SIDES PER SYSTEM AND GREATER THAN 1
    Transform tR11 = Transform.Rotation((dAngDeg / 180.0) * Math.PI, new Vector3d(0.0, 0.0, 1.0), new Point3d(0.0, 0.0, 0.0));
    Transform tT11 = Transform.Translation(new Vector3d(dR1, 0.0, 0.0));
    Transform tT21 = Transform.Translation(new Vector3d(-dSpace, 0.0, 0.0));
    Transform tR21 = Transform.Rotation(((180.0 - dAngDeg) / 180.0) * Math.PI, new Vector3d(0.0, 0.0, 1.0), new Point3d(0.0, 0.0, 0.0));
    Transform tT22 = Transform.Translation(new Vector3d(dR1, 0.0, 0.0));
    for(i = 0; i < lsL_AU1.Count; i++)
    {
      Line L11 = lsL_AU1[i];
      L11.Transform(tR11);
      L11.Transform(tT11);
      lsL_AU1[i] = L11;
    }
    for(i = 0; i < lsL_AU2.Count; i++)
    {
      Line L22 = lsL_AU2[i];
      L22.Transform(tT21);
      L22.Transform(tR21);
      L22.Transform(tT22);
      lsL_AU2[i] = L22;
    }
    // MERGE ALL PARTS TO FORM NEW UNICEL-3
    List<Line> lsL_AU3 = new List<Line>();
    lsL_AU3.AddRange(lsL_AU1);
    lsL_AU3.AddRange(lsL_AU2);
    List<Line> lsL_AU3R = new List<Line>();
    List<Line> lsL_AU3T = new List<Line>();
    int iRadialInstance = iNumH / 2;
    double dTotaldAngDeg = 360.0;
    double dEachAngDeg = dTotaldAngDeg / (double) iRadialInstance;
    for(i = 0; i < iRadialInstance; i++)
    {
      double dAngRotDeg = (double) i * dEachAngDeg;
      for(j = 0; j < lsL_AU3.Count; j++)
      {
        Line L31 = lsL_AU3[j];
        Transform tR31 = Transform.Rotation((dAngRotDeg / 180.0) * Math.PI, new Vector3d(0.0, 0.0, 1.0), new Point3d(0.0, 0.0, 0.0));
        L31.Transform(tR31);
        lsL_AU3R.Add(L31);
      }
    }
    int iLinearInstance = iNumV;
    double dEachDistance = 2.0 * dSpace;
    for(i = 0; i < iLinearInstance; i++)
    {
      double dDistance = (double) i * dEachDistance;
      for(j = 0; j < lsL_AU3R.Count; j++)
      {
        Line L32 = lsL_AU3R[j];
        Transform tT31 = Transform.Translation(new Vector3d(0.0, 0.0, dDistance));
        L32.Transform(tT31);
        lsL_AU3T.Add(L32);
      }
    }
    List<Line> lsL_BC = lsL_AU3T;

    // Top & Bottom Rings
    List<Point3d> lsPT_BR = new List<Point3d>();
    List<Point3d> lsPT_TR = new List<Point3d>();
    List<Line> lsL_BR = new List<Line>();
    List<Line> lsL_TR = new List<Line>();
    for(i = 0; i < iNumH + 1; i++) // + 1 to add the end point to the list in order to close the polyline
    {
      lsPT_BR.Add(new Point3d(dR1 * Math.Sin((double) i * 2.0 * dAngStpRad), dR1 * Math.Cos((double) i * 2.0 * dAngStpRad), 0.0 * (double) iNumV * 2.0 * dSpace));
      lsPT_TR.Add(new Point3d(dR1 * Math.Sin((double) i * 2.0 * dAngStpRad), dR1 * Math.Cos((double) i * 2.0 * dAngStpRad), 1.0 * (double) iNumV * 2.0 * dSpace));
    }
    for(i = 0; i < iNumH + 0; i++)
    {
      lsL_BR.Add(new Line(lsPT_BR[i], lsPT_BR[i + 1]));
      lsL_TR.Add(new Line(lsPT_TR[i], lsPT_TR[i + 1]));
    }

    // Unit Cell Mesh
    Mesh M_UC_OPEN = new Mesh();
    Mesh M_UC_CLOSE = new Mesh();
    List<Mesh> lsM_UC_OPEN = new List<Mesh>();
    List<Mesh> lsM_UC_CLOSE = new List<Mesh>();
    double dX2 = dSpace / (Math.Sqrt(2.0) + 2.0);

    // Four Exterior Triangles
    Mesh M_UCET11 = new Mesh();
    M_UCET11.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace));
    M_UCET11.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) - new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) ));
    M_UCET11.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) - new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) ));
    M_UCET11.Faces.AddFace(0, 2, 1);
    Mesh M_UCET12 = new Mesh();
    M_UCET12.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace));
    M_UCET12.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) - new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) ));
    M_UCET12.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) - new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) ));
    M_UCET12.Faces.AddFace(0, 1, 2);
    Mesh M_UCET13 = new Mesh();
    M_UCET13.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace));
    M_UCET13.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) - new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) ));
    M_UCET13.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) - new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) ));
    M_UCET13.Faces.AddFace(0, 1, 2);
    Mesh M_UCET14 = new Mesh();
    M_UCET14.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace));
    M_UCET14.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) - new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) ));
    M_UCET14.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) - new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) ));
    M_UCET14.Faces.AddFace(0, 2, 1);

    // Three Interior Quads
    Mesh M_UCIQ11 = new Mesh();
    M_UCIQ11.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) - new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) ));
    M_UCIQ11.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) - new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) ));
    M_UCIQ11.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) - new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) ));
    M_UCIQ11.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) - new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) ));
    M_UCIQ11.Faces.AddFace(0, 1, 3, 2);
    M_UCIQ11.Flip(true, true, true);
    Mesh M_UCIQ12 = new Mesh();
    M_UCIQ12.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) - new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) ));
    M_UCIQ12.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) - new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) ));
    M_UCIQ12.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) - new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) ));
    M_UCIQ12.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) - new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) ));
    M_UCIQ12.Faces.AddFace(0, 1, 3, 2);
    M_UCIQ12.Flip(true, true, true);
    Mesh M_UCIQ13 = new Mesh();
    M_UCIQ13.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 0.0 * dSpace) - new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) ));
    M_UCIQ13.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 0.0 * dSpace) - new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) ));
    M_UCIQ13.Vertices.Add(new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) - new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) ));
    M_UCIQ13.Vertices.Add(new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) + dX2 * ( new Point3d(lsPT_BR[0].X, lsPT_BR[0].Y, 1.0 * dSpace) - new Point3d(lsPT_BR[1].X, lsPT_BR[1].Y, 1.0 * dSpace) ));
    M_UCIQ13.Faces.AddFace(0, 1, 3, 2);
    M_UCIQ13.Flip(true, true, true);

    lsM_UC_OPEN.Add(M_UCET11);
    lsM_UC_OPEN.Add(M_UCET12);
    lsM_UC_OPEN.Add(M_UCET13);
    lsM_UC_OPEN.Add(M_UCET14);
    M_UC_OPEN.Append(lsM_UC_OPEN);

    lsM_UC_CLOSE.Add(M_UCET11);
    lsM_UC_CLOSE.Add(M_UCET12);
    lsM_UC_CLOSE.Add(M_UCET13);
    lsM_UC_CLOSE.Add(M_UCET14);
    lsM_UC_CLOSE.Add(M_UCIQ11);
    lsM_UC_CLOSE.Add(M_UCIQ12);
    lsM_UC_CLOSE.Add(M_UCIQ13);
    M_UC_CLOSE.Append(lsM_UC_CLOSE);

    // Assemble Unit Cell
    Mesh M_UC = new Mesh();
    double dAngRotDegUnit = 0.5 * dEachAngDeg;
    Transform T_ROT_UNIT = Transform.Rotation((dAngRotDegUnit / 180.0) * Math.PI, new Vector3d(0.0, 0.0, 1.0), new Point3d(0.0, 0.0, 0.0));
    double dDistanceUnit = 0.5 * dEachDistance;
    Transform T_TRA_UNIT = Transform.Translation(new Vector3d(0.0, 0.0, dDistanceUnit));
    if(bOpening == true)
    {
      Mesh M_UC11 = M_UC_CLOSE.DuplicateMesh();
      Mesh M_UC12 = M_UC_OPEN.DuplicateMesh();
      Mesh M_UC21 = M_UC_OPEN.DuplicateMesh();
      Mesh M_UC22 = M_UC_CLOSE.DuplicateMesh();
      M_UC12.Transform(T_ROT_UNIT);
      M_UC21.Transform(T_TRA_UNIT);
      M_UC22.Transform(T_ROT_UNIT);
      M_UC22.Transform(T_TRA_UNIT);
      M_UC.Append(M_UC11);
      M_UC.Append(M_UC12);
      M_UC.Append(M_UC21);
      M_UC.Append(M_UC22);
    }
    else
    {
      Mesh M_UC11 = M_UC_CLOSE.DuplicateMesh();
      Mesh M_UC12 = M_UC_CLOSE.DuplicateMesh();
      Mesh M_UC21 = M_UC_CLOSE.DuplicateMesh();
      Mesh M_UC22 = M_UC_CLOSE.DuplicateMesh();
      M_UC12.Transform(T_ROT_UNIT);
      M_UC21.Transform(T_TRA_UNIT);
      M_UC22.Transform(T_ROT_UNIT);
      M_UC22.Transform(T_TRA_UNIT);
      M_UC.Append(M_UC11);
      M_UC.Append(M_UC12);
      M_UC.Append(M_UC21);
      M_UC.Append(M_UC22);
    }

    // Assemble Mesh to Base Cylinder
    List<Mesh> lsM_UC_R = new List<Mesh>();
    for(i = 0; i < iRadialInstance; i++)
    {
      double dAngRotDeg = (double) i * 1.0 * dEachAngDeg;
      Mesh M_UC_BASE = M_UC.DuplicateMesh();
      Transform T_ROT = Transform.Rotation((dAngRotDeg / 180.0) * Math.PI, new Vector3d(0.0, 0.0, 1.0), new Point3d(0.0, 0.0, 0.0));
      M_UC_BASE.Transform(T_ROT);
      lsM_UC_R.Add(M_UC_BASE);
    }
    Mesh M_UC_R = new Mesh();
    M_UC_R.Append(lsM_UC_R);

    List<Mesh> lsM_UC_T = new List<Mesh>();
    for(i = 0; i < iLinearInstance; i++)
    {
      double dDistance = (double) i * 1.0 * dEachDistance;
      Mesh M_UC_R_BASE = M_UC_R.DuplicateMesh();
      Transform T_TRA = Transform.Translation(new Vector3d(0.0, 0.0, dDistance));
      M_UC_R_BASE.Transform(T_TRA);
      lsM_UC_T.Add(M_UC_R_BASE);
    }
    Mesh M_BC = new Mesh();
    M_BC.Append(lsM_UC_T);
    M_BC.HealNakedEdges(0.1);
    M_BC.Compact();

    Mesh M_BC_OUT = new Mesh();
    if (bThick == true)
    {
      M_BC_OUT = M_BC.Offset(1.0 * dThick, true);
    }
    else
    {
      M_BC_OUT = M_BC;
    }

    // Helix and Support
    List<Line> lsL_HL = new List<Line>();
    List<Line> lsL_HR = new List<Line>();
    List<Line> lsL_SL = new List<Line>();
    List<Line> lsL_SR = new List<Line>();
    List<Mesh> lsM_HL = new List<Mesh>();
    List<Mesh> lsM_HR = new List<Mesh>();
    List<Point3d> lsP_HL = new List<Point3d>();
    List<Point3d> lsP_HR = new List<Point3d>();
    if (bRidge == true)
    {
      int k = 0;
      int u = 0;

      //
      // Loop Row
      //

      for (i = 0; i < iNumV * 2; i++)
      {

        //
        // in case of matrix shorter than the sea sponge, pattern needs to be repeated, that's why i>=32,u=i-32*int(i/32) and the condition on j>=32
        //

        if (i >= 32)
        {
          u = i - 32 * (int) (i / 32);
        }
        else
        {
          u = i;
        }

        //
        // Loop Column
        //

        for (j = 0; j < iNumH; j++)
          // for(j = 0; j < 1; j++)
        {
          if (j >= 32){
            k = j - 32 * (int) (j / 32);
          }
          if (j < 32){
            k = j;
          }
          int idX = (int) dX[u][k];
          List<Mesh> lsM = new List<Mesh>();
          List<Point3d> lsP = new List<Point3d>();

          if ( (idX == 1) || (idX == 11) || (idX == 13) )
          {
            if(lsIDX[idX] == 1)
            {
              if (bLeft == true)
              {
                lsL_HL.AddRange(lsL_HelixLeft(i, j, idX, iNumV, dAngStpRad, dR1, dR2, dSpace, bCap, out lsM, out lsP));
                lsL_SL.AddRange(lsL_SupportLeft(i, j, idX, iNumV, dAngStpRad, dR1, dR2, dSpace));
                lsM_HL.AddRange(lsM);
                lsP_HL.AddRange(lsP);
                lsM.Clear();
                lsP.Clear();
              }
            }
          }

          else if ( (idX == 2) || (idX == 12) || (idX == 14) )
          {
            if(lsIDX[idX] == 1)
            {
              if (bRight == true)
              {
                lsL_HR.AddRange(lsL_HelixRight(i, j, idX, iNumV, dAngStpRad, dR1, dR2, dSpace, bCap, out lsM, out lsP));
                lsL_SR.AddRange(lsL_SupportRight(i, j, idX, iNumV, dAngStpRad, dR1, dR2, dSpace));
                lsM_HR.AddRange(lsM);
                lsP_HR.AddRange(lsP);
                lsM.Clear();
                lsP.Clear();
              }
            }
          }

          else if ( (idX == 3) || (idX == 4) || (idX == 5) || (idX == 6) || (idX == 7) || (idX == 8) || (idX == 9) || (idX == 10) || (idX == 15) )
          {
            if(lsIDX[idX] == 1)
            {
              if (bLeft == true)
              {
                lsL_HL.AddRange(lsL_HelixLeft(i, j, idX, iNumV, dAngStpRad, dR1, dR2, dSpace, bCap, out lsM, out lsP));
                lsL_SL.AddRange(lsL_SupportLeft(i, j, idX, iNumV, dAngStpRad, dR1, dR2, dSpace));
                lsM_HL.AddRange(lsM);
                lsP_HL.AddRange(lsP);
                lsM.Clear();
                lsP.Clear();
              }
              if (bRight == true)
              {
                lsL_HR.AddRange(lsL_HelixRight(i, j, idX, iNumV, dAngStpRad, dR1, dR2, dSpace, bCap, out lsM, out lsP));
                lsL_SR.AddRange(lsL_SupportRight(i, j, idX, iNumV, dAngStpRad, dR1, dR2, dSpace));
                lsM_HR.AddRange(lsM);
                lsP_HR.AddRange(lsP);
                lsM.Clear();
                lsP.Clear();
              }
            }
          }
        }
      }
    }

    LBC = lsL_BC;
    MBC = M_BC_OUT;
    LHL = lsL_HL;
    LHR = lsL_HR;
    LSL = lsL_SL;
    LSR = lsL_SR;
    MHL = lsM_HL;
    MHR = lsM_HR;
    PHL = lsP_HL;
    PHR = lsP_HR;

  }

  // <Custom additional code> 

  //
  // Debug Parameter
  //

  public bool bLeft = true;
  public bool bRight = true;
  public bool bTop = false;
  public bool bMid = true;
  public bool bBottom = true;

  //
  // Helix Left
  //

  List<Line> lsL_HelixLeft(int iLine, int iColumn, int iType, int iNumV, double dAngStpRad, double dR1, double dR2, double dSpace, bool bCap, out List<Mesh> lsM, out List<Point3d> lsP) // iNumV = iNumV
  {
    List<Line> lsL_Return = new List<Line>();
    List<Mesh> lsM_Out = new List<Mesh>();
    List<Point3d> lsP_Out = new List<Point3d>();
    double dAngPosRad = -((double) iColumn + 1.0) * 2.0 * dAngStpRad;

    if (
      ((iType == 1) && (iLine != 0))
      || (iType == 4)
      || (iType == 5)
      || (iType == 15)
      )
    {

      //
      // Middle Segments
      //

      if (bMid == true)
      {

        //
        // Line
        //

        // Outer Center
        Point3d P11 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d P12 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d P13 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(P11);
        lsP_Out.Add(P12);
        lsP_Out.Add(P13);
        Line L11 = new Line(P11, P12);
        Line L12 = new Line(P12, P13);
        lsL_Return.Add(L11);
        lsL_Return.Add(L12);

        //
        // Points
        //

        // Outer Center (Same as Helix Points)
        Point3d PMOC11 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d PMOC12 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d PMOC13 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(PMOC11);
        lsP_Out.Add(PMOC12);
        lsP_Out.Add(PMOC13);
        // Inner Center
        Point3d PMIC11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIC12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIC13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIC11);
        lsP_Out.Add(PMIC12);
        lsP_Out.Add(PMIC13);
        // Inner Top
        Point3d PMIT11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
        Point3d PMIT12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 1.0 + 0.5 * dSpace);
        Point3d PMIT13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIT11);
        lsP_Out.Add(PMIT12);
        lsP_Out.Add(PMIT13);
        // Inner Top Before
        Point3d PMITB11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad - 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 2.0 * dAngStpRad), (double) iLine + 1.0 - 1.0 * dSpace);
        Point3d PMITB12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad - 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 1.0 * dAngStpRad), (double) iLine + 1.0 - 0.5 * dSpace);
        Point3d PMITB13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad - 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 0.0 * dAngStpRad), (double) iLine + 1.0 - 0.0 * dSpace);
        lsP_Out.Add(PMITB11);
        lsP_Out.Add(PMITB12);
        lsP_Out.Add(PMITB13);
        // Inner Bottom
        Point3d PMIB11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine - 1.0 + 0.0 * dSpace);
        Point3d PMIB12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine - 1.0 + 0.5 * dSpace);
        Point3d PMIB13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine - 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIB11);
        lsP_Out.Add(PMIB12);
        lsP_Out.Add(PMIB13);
        // Inner Bottom After
        Point3d PMIBA11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIBA12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 3.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 3.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIBA13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 4.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 4.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIBA11);
        lsP_Out.Add(PMIBA12);
        lsP_Out.Add(PMIBA13);

        //
        // Mesh
        //

        //
        // Front Faces
        //

        // Mesh Top 11
        Mesh MT11 = new Mesh();
        MT11.Vertices.Add(PMOC11);
        MT11.Vertices.Add(PMOC12);
        MT11.Vertices.Add(PMITB11);
        MT11.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MT11);
        // Mesh Top 12
        Mesh MT12 = new Mesh();
        MT12.Vertices.Add(PMOC12);
        MT12.Vertices.Add(PMITB12);
        MT12.Vertices.Add(PMITB11);
        MT12.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MT12);
        // Mesh Top 21
        Mesh MT21 = new Mesh();
        MT21.Vertices.Add(PMOC12);
        MT21.Vertices.Add(PMOC13);
        MT21.Vertices.Add(PMITB12);
        MT21.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MT21);
        // Mesh Top 22
        Mesh MT22 = new Mesh();
        MT22.Vertices.Add(PMOC13);
        MT22.Vertices.Add(PMITB13);
        MT22.Vertices.Add(PMITB12);
        MT22.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MT22);
        // Mesh Bottom 11
        Mesh MB11 = new Mesh();
        MB11.Vertices.Add(PMOC11);
        MB11.Vertices.Add(PMIB11);
        MB11.Vertices.Add(PMOC12);
        MB11.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB11);
        // Mesh Bottom 12
        Mesh MB12 = new Mesh();
        MB12.Vertices.Add(PMOC12);
        MB12.Vertices.Add(PMIB11);
        MB12.Vertices.Add(PMIB12);
        MB12.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB12);
        // Mesh Bottom 21
        Mesh MB21 = new Mesh();
        MB21.Vertices.Add(PMOC12);
        MB21.Vertices.Add(PMOC13);
        MB21.Vertices.Add(PMIB12);
        MB21.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB21);
        // Mesh Bottom 22
        Mesh MB22 = new Mesh();
        MB22.Vertices.Add(PMOC13);
        MB22.Vertices.Add(PMIB12);
        MB22.Vertices.Add(PMIB13);
        MB22.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB22);

        if (bCap == true)
        {

          //
          // Back Faces
          //

          // Mesh Top 11 Back
          Mesh MT11B = new Mesh();
          MT11B.Vertices.Add(PMIC11);
          MT11B.Vertices.Add(PMIC12);
          MT11B.Vertices.Add(PMITB11);
          MT11B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MT11B);
          // Mesh Top 12 Back
          Mesh MT12B = new Mesh();
          MT12B.Vertices.Add(PMIC12);
          MT12B.Vertices.Add(PMITB12);
          MT12B.Vertices.Add(PMITB11);
          MT12B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MT12B);
          // Mesh Top 21 Back
          Mesh MT21B = new Mesh();
          MT21B.Vertices.Add(PMIC12);
          MT21B.Vertices.Add(PMIC13);
          MT21B.Vertices.Add(PMITB12);
          MT21B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MT21B);
          // Mesh Top 22 Back
          Mesh MT22B = new Mesh();
          MT22B.Vertices.Add(PMIC13);
          MT22B.Vertices.Add(PMITB13);
          MT22B.Vertices.Add(PMITB12);
          MT22B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MT22B);
          // Mesh Bottom 11 Back
          Mesh MB11B = new Mesh();
          MB11B.Vertices.Add(PMIC11);
          MB11B.Vertices.Add(PMIB11);
          MB11B.Vertices.Add(PMIC12);
          MB11B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MB11B);
          // Mesh Bottom 12 Back
          Mesh MB12B = new Mesh();
          MB12B.Vertices.Add(PMIC12);
          MB12B.Vertices.Add(PMIB11);
          MB12B.Vertices.Add(PMIB12);
          MB12B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MB12B);
          // Mesh Bottom 21 Back
          Mesh MB21B = new Mesh();
          MB21B.Vertices.Add(PMIC12);
          MB21B.Vertices.Add(PMIC13);
          MB21B.Vertices.Add(PMIB12);
          MB21B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MB21B);
          // Mesh Bottom 22 Back
          Mesh MB22B = new Mesh();
          MB22B.Vertices.Add(PMIC13);
          MB22B.Vertices.Add(PMIB12);
          MB22B.Vertices.Add(PMIB13);
          MB22B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MB22B);

          //
          // Capping Faces
          //

          // Mesh Top 11 Cap Start
          Mesh MT11CS = new Mesh();
          MT11CS.Vertices.Add(PMOC11);
          MT11CS.Vertices.Add(PMIC11);
          MT11CS.Vertices.Add(PMITB11);
          MT11CS.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MT11CS);
          // Mesh Bottom 11 Cap Start
          Mesh MB11CS = new Mesh();
          MB11CS.Vertices.Add(PMOC11);
          MB11CS.Vertices.Add(PMIC11);
          MB11CS.Vertices.Add(PMIB11);
          MB11CS.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB11CS);
          // Mesh Top 22 Cap End
          Mesh MT22CE = new Mesh();
          MT22CE.Vertices.Add(PMOC13);
          MT22CE.Vertices.Add(PMIC13);
          MT22CE.Vertices.Add(PMIT11);
          MT22CE.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT22CE);
          // Mesh Bottom 22 Cap End
          Mesh MB22CE = new Mesh();
          MB22CE.Vertices.Add(PMOC13);
          MB22CE.Vertices.Add(PMIC13);
          MB22CE.Vertices.Add(PMIB13);
          MB22CE.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MB22CE);

        }

        //
        // Combine Meshes and Add to List
        //

        Mesh MJ = new Mesh();
        MJ.Append(lsM_Out);
        MJ.UnifyNormals();
        lsM_Out.Clear();
        lsM_Out.Add(MJ);

      }
    }

    else if (
      ((iType == 1) && (iLine + 1 == 2 * iNumV))
      || ((iType == 4) && (iLine + 1 == 2 * iNumV))
      || ((iType == 5) && (iLine + 1 == 2 * iNumV))
      || ((iType == 15) && (iLine + 1 == 2 * iNumV))
      || (iType == 6)
      || (iType == 7)
      || (iType == 10)
      || (iType == 11)
      )
    {

      //
      // Top Segments
      //

      if (bTop == true)
      {

        //
        // Line
        //

        // Outer Center
        Point3d P11 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d P12 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d P13 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(P11);
        lsP_Out.Add(P12);
        lsP_Out.Add(P13);
        Line L11 = new Line(P11, P12);
        Line L12 = new Line(P12, P13);
        lsL_Return.Add(L11);
        // lsL_Return.Add(L12);

        //
        // Points
        //

        // Outer Center (Same as Helix Points)
        Point3d PMOC11 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d PMOC12 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d PMOC13 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(PMOC11);
        lsP_Out.Add(PMOC12);
        lsP_Out.Add(PMOC13);
        // Inner Center
        Point3d PMIC11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIC12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIC13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIC11);
        lsP_Out.Add(PMIC12);
        lsP_Out.Add(PMIC13);
        // Inner Top
        Point3d PMIT11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
        Point3d PMIT12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 1.0 + 0.5 * dSpace);
        Point3d PMIT13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIT11);
        lsP_Out.Add(PMIT12);
        lsP_Out.Add(PMIT13);
        // Inner Top Before
        Point3d PMITB11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad - 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 2.0 * dAngStpRad), (double) iLine + 1.0 - 1.0 * dSpace);
        Point3d PMITB12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad - 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 1.0 * dAngStpRad), (double) iLine + 1.0 - 0.5 * dSpace);
        Point3d PMITB13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad - 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 0.0 * dAngStpRad), (double) iLine + 1.0 - 0.0 * dSpace);
        lsP_Out.Add(PMITB11);
        lsP_Out.Add(PMITB12);
        lsP_Out.Add(PMITB13);
        // Inner Bottom
        Point3d PMIB11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine - 1.0 + 0.0 * dSpace);
        Point3d PMIB12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine - 1.0 + 0.5 * dSpace);
        Point3d PMIB13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine - 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIB11);
        lsP_Out.Add(PMIB12);
        lsP_Out.Add(PMIB13);
        // Inner Bottom After
        Point3d PMIBA11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIBA12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 3.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 3.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIBA13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 4.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 4.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIBA11);
        lsP_Out.Add(PMIBA12);
        lsP_Out.Add(PMIBA13);

        //
        // Mesh
        //

        //
        // Front Faces
        //

        // Mesh Top 1
        Mesh MT11 = new Mesh();
        MT11.Vertices.Add(PMOC11);
        MT11.Vertices.Add(PMITB12);
        MT11.Vertices.Add(PMITB11);
        MT11.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MT11);
        // Mesh Top 2
        Mesh MT12 = new Mesh();
        MT12.Vertices.Add(PMOC11);
        MT12.Vertices.Add(PMOC12);
        MT12.Vertices.Add(PMITB12);
        MT12.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MT12);
        // Mesh Bottom 1
        Mesh MB11 = new Mesh();
        MB11.Vertices.Add(PMOC11);
        MB11.Vertices.Add(PMIB11);
        MB11.Vertices.Add(PMIB12);
        MB11.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB11);
        // Mesh Bottom 2
        Mesh MB12 = new Mesh();
        MB12.Vertices.Add(PMOC11);
        MB12.Vertices.Add(PMIB12);
        MB12.Vertices.Add(PMOC12);
        MB12.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB12);

        if (bCap == true)
        {

          //
          // Back Faces
          //

          // Mesh Top 1 Back
          Mesh MT11B = new Mesh();
          MT11B.Vertices.Add(PMIC11);
          MT11B.Vertices.Add(PMITB11);
          MT11B.Vertices.Add(PMITB12);
          MT11B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT11B);
          // Mesh Top 2 Back
          Mesh MT12B = new Mesh();
          MT12B.Vertices.Add(PMIC11);
          MT12B.Vertices.Add(PMITB12);
          MT12B.Vertices.Add(PMIC12);
          MT12B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT12B);
          // Mesh Bottom 1 Back
          Mesh MB11B = new Mesh();
          MB11B.Vertices.Add(PMIC11);
          MB11B.Vertices.Add(PMIB12);
          MB11B.Vertices.Add(PMIB11);
          MB11B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB11B);
          // Mesh Bottom 2 Back
          Mesh MB12B = new Mesh();
          MB12B.Vertices.Add(PMIC11);
          MB12B.Vertices.Add(PMIC12);
          MB12B.Vertices.Add(PMIB12);
          MB12B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB12B);

          //
          // Capping Faces
          //

          // Mesh Top 1 Side
          Mesh MC11TS = new Mesh();
          MC11TS.Vertices.Add(PMIC11);
          MC11TS.Vertices.Add(PMOC11);
          MC11TS.Vertices.Add(PMITB11);
          MC11TS.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MC11TS);
          // Mesh Bottom 1 Side
          Mesh MC11BS = new Mesh();
          MC11BS.Vertices.Add(PMIC11);
          MC11BS.Vertices.Add(PMIB11);
          MC11BS.Vertices.Add(PMOC11);
          MC11BS.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MC11BS);
          // Mesh Top 2 Side
          Mesh MC12TS = new Mesh();
          MC12TS.Vertices.Add(PMIC12);
          MC12TS.Vertices.Add(PMITB12);
          MC12TS.Vertices.Add(PMOC12);
          MC12TS.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MC12TS);
          // Mesh Bottom 2 Side
          Mesh MC12BS = new Mesh();
          MC12BS.Vertices.Add(PMIC12);
          MC12BS.Vertices.Add(PMOC12);
          MC12BS.Vertices.Add(PMIB12);
          MC12BS.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MC12BS);

          if ((iLine + 1 != 2 * iNumV))
          {

            //
            // Top Intersection Triangle
            //

            List<Mesh> lsMT = new List<Mesh>();
            // Mesh 1 Side
            Mesh MT11S = new Mesh();
            MT11S.Vertices.Add(PMOC12);
            MT11S.Vertices.Add(PMIT12);
            MT11S.Vertices.Add(PMIC12);
            MT11S.Faces.AddFace(0, 1, 2);
            //lsM_Out.Add(MT11S);
            lsMT.Add(MT11S);
            // Mesh 1 Front Bottom
            Mesh MT11FB = new Mesh();
            MT11FB.Vertices.Add(PMOC12);
            MT11FB.Vertices.Add(PMITB12);
            MT11FB.Vertices.Add(PMIT11);
            MT11FB.Faces.AddFace(0, 1, 2);
            //lsM_Out.Add(MT11FB);
            lsMT.Add(MT11FB);
            // Mesh 1 Front Top
            Mesh MT11FT = new Mesh();
            MT11FT.Vertices.Add(PMOC12);
            MT11FT.Vertices.Add(PMIT11);
            MT11FT.Vertices.Add(PMIT12);
            MT11FT.Faces.AddFace(0, 1, 2);
            //lsM_Out.Add(MT11FT);
            lsMT.Add(MT11FT);
            // Mesh 1 Rear Bottom
            Mesh MT11RB = new Mesh();
            MT11RB.Vertices.Add(PMIC12);
            MT11RB.Vertices.Add(PMITB12);
            MT11RB.Vertices.Add(PMIT11);
            MT11RB.Faces.AddFace(0, 1, 2);
            //lsM_Out.Add(MT11RB);
            lsMT.Add(MT11RB);
            // Mesh 1 Rear Top
            Mesh MT11RT = new Mesh();
            MT11RT.Vertices.Add(PMIC12);
            MT11RT.Vertices.Add(PMIT12);
            MT11RT.Vertices.Add(PMIT11);
            MT11RT.Faces.AddFace(0, 1, 2);
            //lsM_Out.Add(MT11RT);
            lsMT.Add(MT11RT);
            // Mesh 1 Bottom
            Mesh MT11BB = new Mesh();
            MT11BB.Vertices.Add(PMOC12);
            MT11BB.Vertices.Add(PMITB12);
            MT11BB.Vertices.Add(PMIC12);
            MT11BB.Faces.AddFace(0, 1, 2);
            //lsM_Out.Add(MT11BB);
            lsMT.Add(MT11BB);

            //
            // Combine Meshes and Add to List
            //

            Mesh MJT = new Mesh();
            MJT.Append(lsMT);
            MJT.UnifyNormals();
            lsM_Out.Add(MJT);
            lsMT.Clear();
          }

        }

        //
        // Combine Meshes and Add to List
        //

        Mesh MJ = new Mesh();
        MJ.Append(lsM_Out);
        //MJ.UnifyNormals();
        lsM_Out.Clear();
        lsM_Out.Add(MJ);

      }
    }

    else if (
      ((iType == 3) && (iLine + 1 != 2 * iNumV) && (iLine != 1))
      || (iType == 8)
      || (iType == 9)
      || (iType == 13)
      || ((iType == 1 && iLine == 0)) )
    {

      //
      // Bottom Segments
      //

      if (bBottom == true)
      {

        //
        // Lines
        //

        // Outer Center (Helix Points)
        Point3d P11 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d P12 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d P13 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(P11);
        lsP_Out.Add(P12);
        lsP_Out.Add(P12);
        Line L11 = new Line(P11, P12);
        Line L12 = new Line(P12, P13);
        //lsL_Return.Add(L11);
        lsL_Return.Add(L12);

        //
        // Points
        //

        // Outer Center (Same as Helix Points)
        Point3d PMOC11 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d PMOC12 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d PMOC13 = new Point3d(-dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(PMOC11);
        lsP_Out.Add(PMOC12);
        lsP_Out.Add(PMOC13);
        // Inner Center
        Point3d PMIC11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIC12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIC13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIC11);
        lsP_Out.Add(PMIC12);
        lsP_Out.Add(PMIC13);
        // Inner Top
        Point3d PMIT11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
        Point3d PMIT12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 1.0 + 0.5 * dSpace);
        Point3d PMIT13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIT11);
        lsP_Out.Add(PMIT12);
        lsP_Out.Add(PMIT13);
        // Inner Top Before
        Point3d PMITB11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad - 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 2.0 * dAngStpRad), (double) iLine + 1.0 - 1.0 * dSpace);
        Point3d PMITB12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad - 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 1.0 * dAngStpRad), (double) iLine + 1.0 - 0.5 * dSpace);
        Point3d PMITB13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad - 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 0.0 * dAngStpRad), (double) iLine + 1.0 - 0.0 * dSpace);
        lsP_Out.Add(PMITB11);
        lsP_Out.Add(PMITB12);
        lsP_Out.Add(PMITB13);
        // Inner Bottom
        Point3d PMIB11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine - 1.0 + 0.0 * dSpace);
        Point3d PMIB12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine - 1.0 + 0.5 * dSpace);
        Point3d PMIB13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine - 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIB11);
        lsP_Out.Add(PMIB12);
        lsP_Out.Add(PMIB13);
        // Inner Bottom After
        Point3d PMIBA11 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIBA12 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 3.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 3.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIBA13 = new Point3d(-dR1 * Math.Sin(1.0 * dAngPosRad + 4.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 4.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIBA11);
        lsP_Out.Add(PMIBA12);
        lsP_Out.Add(PMIBA13);

        //
        // Mesh
        //

        //
        // Front Faces
        //

        // Mesh Top 1
        Mesh MT11 = new Mesh();
        MT11.Vertices.Add(PMOC12);
        MT11.Vertices.Add(PMOC13);
        MT11.Vertices.Add(PMITB13);
        MT11.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MT11);
        // Mesh Bottom 1
        Mesh MB11 = new Mesh();
        MB11.Vertices.Add(PMOC12);
        MB11.Vertices.Add(PMIB13);
        MB11.Vertices.Add(PMOC13);
        MB11.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB11);
        // Mesh Top 0
        Mesh MT10 = new Mesh();
        MT10.Vertices.Add(PMIC11);
        MT10.Vertices.Add(PMOC12);
        MT10.Vertices.Add(PMIT11);
        MT10.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MT10);
        // Mesh Bottom 0
        Mesh MB10 = new Mesh();
        MB10.Vertices.Add(PMIC11);
        MB10.Vertices.Add(PMIB13);
        MB10.Vertices.Add(PMOC12);
        MB10.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB10);

        if (bCap == true)
        {

          //
          // Back Faces
          //

          // Mesh Top 0 Back
          Mesh MT10B = new Mesh();
          MT10B.Vertices.Add(PMIC11);
          MT10B.Vertices.Add(PMIT11);
          MT10B.Vertices.Add(PMIC12);
          MT10B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT10B);
          // Mesh Top 1 Back
          Mesh MT11B = new Mesh();
          MT11B.Vertices.Add(PMIC12);
          MT11B.Vertices.Add(PMIC13);
          MT11B.Vertices.Add(PMIT11);
          MT11B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT11B);
          // Mesh Bottom 0 Back
          Mesh MB10B = new Mesh();
          MB10B.Vertices.Add(PMIC12);
          MB10B.Vertices.Add(PMIB13);
          MB10B.Vertices.Add(PMIC11);
          MB10B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB10B);
          // Mesh Bottom 1 Back
          Mesh MB11B = new Mesh();
          MB11B.Vertices.Add(PMIC12);
          MB11B.Vertices.Add(PMIC13);
          MB11B.Vertices.Add(PMIB13);
          MB11B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB11B);

          //
          // Capping Faces
          //

          // Mesh Top 1 Side
          Mesh MT11S = new Mesh();
          MT11S.Vertices.Add(PMOC13);
          MT11S.Vertices.Add(PMIC13);
          MT11S.Vertices.Add(PMIT11);
          MT11S.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT11S);
          // Mesh Bottom 1 Side
          Mesh MB11S = new Mesh();
          MB11S.Vertices.Add(PMOC13);
          MB11S.Vertices.Add(PMIB13);
          MB11S.Vertices.Add(PMIC13);
          MB11S.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB11S);

        }

        //
        // Combine Meshe and Add to List
        //

        Mesh MJ = new Mesh();
        MJ.Append(lsM_Out);
        MJ.UnifyNormals();
        lsM_Out.Clear();
        lsM_Out.Add(MJ);

        //
        // Gap Fillling Block
        //

        if ((bCap == true) && (iLine != 0))
        {
          List<Mesh> lsMF = new List<Mesh>();
          //  Mesh Top
          Mesh MF11T = new Mesh();
          MF11T.Vertices.Add(PMOC12);
          MF11T.Vertices.Add(PMIB13);
          MF11T.Vertices.Add(PMIC11);
          MF11T.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF11T);
          //  Mesh Front
          Mesh MF11F = new Mesh();
          MF11F.Vertices.Add(PMOC12);
          MF11F.Vertices.Add(PMIC11);
          MF11F.Vertices.Add(PMIB12);
          MF11F.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF11F);
          //  Mesh Bottom
          Mesh MF11B = new Mesh();
          MF11B.Vertices.Add(PMOC12);
          MF11B.Vertices.Add(PMIB12);
          MF11B.Vertices.Add(PMIB13);
          MF11B.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF11B);
          //  Mesh Rear
          Mesh MF11R = new Mesh();
          MF11R.Vertices.Add(PMIC11);
          MF11R.Vertices.Add(PMIB12);
          MF11R.Vertices.Add(PMIB13);
          MF11R.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF11R);

          //
          // Combine Meshes and Add to List
          //

          Mesh MFJ = new Mesh();
          MFJ.Append(lsMF);
          MFJ.UnifyNormals();
          lsM_Out.Add(MFJ);
          lsMF.Clear();
        }
      }
    }

    //
    // Output
    //
    lsM = lsM_Out;
    lsP = lsP_Out;
    return lsL_Return;

  }

  //
  // Helix Right
  //

  List<Line> lsL_HelixRight(int iLine, int iColumn, int iType, int iNumV, double dAngStpRad, double dR1, double dR2, double dSpace, bool bCap, out List<Mesh> lsM, out List<Point3d> lsP) // iNumV = iNumV
  {

    List<Line> lsL_Return = new List<Line>();
    List<Mesh> lsM_Out = new List<Mesh>();
    List<Point3d> lsP_Out = new List<Point3d>();
    double dAngPosRad = ((double) iColumn) * 2.0 * dAngStpRad;

    if ( ( (iType == 2) && (iLine != 0) ) || ((iType == 3) && (iLine != 31)) || ((iType == 6)) || ((iType == 15)) )
    {

      //
      // Middle Segments
      //

      if(bMid == true)
      {

        //
        // Line
        //

        // Outer Center (Helix Points)
        Point3d P11 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d P12 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d P13 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        Line L11 = new Line(P11, P12);
        Line L12 = new Line(P12, P13);
        lsL_Return.Add(L11);
        lsL_Return.Add(L12);

        //
        // Point
        //

        // Outer Center (Same as Helix Points)
        Point3d PMOC11 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d PMOC12 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d PMOC13 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(PMOC11);
        lsP_Out.Add(PMOC12);
        lsP_Out.Add(PMOC13);
        // Outer Center Before
        Point3d PMOCB11 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad - 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad - 2.0 * dAngStpRad), (double) iLine - 1.0 * dSpace);
        Point3d PMOCB12 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad - 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad - 1.0 * dAngStpRad), (double) iLine - 0.5 * dSpace);
        Point3d PMOCB13 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad - 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad - 0.0 * dAngStpRad), (double) iLine - 0.0 * dSpace);
        lsP_Out.Add(PMOCB11);
        lsP_Out.Add(PMOCB12);
        lsP_Out.Add(PMOCB13);
        // Inner Center
        Point3d PMIC11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIC12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIC13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIC11);
        lsP_Out.Add(PMIC12);
        lsP_Out.Add(PMIC13);
        // Inner Center Before
        Point3d PMICB11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 2.0 * dAngStpRad), (double) iLine + 0.0 - 1.0 * dSpace);
        Point3d PMICB12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 1.0 * dAngStpRad), (double) iLine + 0.0 - 0.5 * dSpace);
        Point3d PMICB13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 0.0 * dAngStpRad), (double) iLine + 0.0 - 0.0 * dSpace);
        lsP_Out.Add(PMICB11);
        lsP_Out.Add(PMICB12);
        lsP_Out.Add(PMICB13);
        // Inner Top
        Point3d PMIT11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
        Point3d PMIT12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 1.0 + 0.5 * dSpace);
        Point3d PMIT13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIT11);
        lsP_Out.Add(PMIT12);
        lsP_Out.Add(PMIT13);
        // Inner Top Before
        Point3d PMITB11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 2.0 * dAngStpRad), (double) iLine + 1.0 - 1.0 * dSpace);
        Point3d PMITB12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 1.0 * dAngStpRad), (double) iLine + 1.0 - 0.5 * dSpace);
        Point3d PMITB13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 0.0 * dAngStpRad), (double) iLine + 1.0 - 0.0 * dSpace);
        lsP_Out.Add(PMITB11);
        lsP_Out.Add(PMITB12);
        lsP_Out.Add(PMITB13);
        // Inner Bottom
        Point3d PMIB11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine - 1.0 + 0.0 * dSpace);
        Point3d PMIB12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine - 1.0 + 0.5 * dSpace);
        Point3d PMIB13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine - 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIB11);
        lsP_Out.Add(PMIB12);
        lsP_Out.Add(PMIB13);
        // Inner Bottom After
        Point3d PMIBA11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIBA12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 3.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 3.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIBA13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 4.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 4.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIBA11);
        lsP_Out.Add(PMIBA12);
        lsP_Out.Add(PMIBA13);

        //
        // Mesh
        //

        //
        // Front Faces
        //

        // Mesh Top 11
        Mesh MT11 = new Mesh();
        MT11.Vertices.Add(PMOC11);
        MT11.Vertices.Add(PMOC12);
        MT11.Vertices.Add(PMITB11);
        MT11.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MT11);
        // Mesh Top 12
        Mesh MT12 = new Mesh();
        MT12.Vertices.Add(PMOC12);
        MT12.Vertices.Add(PMITB12);
        MT12.Vertices.Add(PMITB11);
        MT12.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MT12);
        // Mesh Top 21
        Mesh MT21 = new Mesh();
        MT21.Vertices.Add(PMOC12);
        MT21.Vertices.Add(PMOC13);
        MT21.Vertices.Add(PMITB12);
        MT21.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MT21);
        // Mesh Top 22
        Mesh MT22 = new Mesh();
        MT22.Vertices.Add(PMOC13);
        MT22.Vertices.Add(PMITB13);
        MT22.Vertices.Add(PMITB12);
        MT22.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MT22);
        // Mesh Bottom 11
        Mesh MB11 = new Mesh();
        MB11.Vertices.Add(PMOC11);
        MB11.Vertices.Add(PMIB11);
        MB11.Vertices.Add(PMOC12);
        MB11.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MB11);
        // Mesh Bottom 12
        Mesh MB12 = new Mesh();
        MB12.Vertices.Add(PMOC12);
        MB12.Vertices.Add(PMIB11);
        MB12.Vertices.Add(PMIB12);
        MB12.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MB12);
        // Mesh Bottom 21
        Mesh MB21 = new Mesh();
        MB21.Vertices.Add(PMOC12);
        MB21.Vertices.Add(PMOC13);
        MB21.Vertices.Add(PMIB12);
        MB21.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB21);
        // Mesh Bottom 22
        Mesh MB22 = new Mesh();
        MB22.Vertices.Add(PMOC13);
        MB22.Vertices.Add(PMIB12);
        MB22.Vertices.Add(PMIB13);
        MB22.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MB22);

        if (bCap == true)
        {
          //
          // Back Faces
          //

          // Mesh Top 11 Back
          Mesh MT11B = new Mesh();
          MT11B.Vertices.Add(PMIC11);
          MT11B.Vertices.Add(PMIC12);
          MT11B.Vertices.Add(PMITB11);
          MT11B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT11B);
          // Mesh Top 12 Back
          Mesh MT12B = new Mesh();
          MT12B.Vertices.Add(PMIC12);
          MT12B.Vertices.Add(PMITB12);
          MT12B.Vertices.Add(PMITB11);
          MT12B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT12B);
          // Mesh Top 21 Back
          Mesh MT21B = new Mesh();
          MT21B.Vertices.Add(PMIC12);
          MT21B.Vertices.Add(PMIC13);
          MT21B.Vertices.Add(PMITB12);
          MT21B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT21B);
          // Mesh Top 22 Back
          Mesh MT22B = new Mesh();
          MT22B.Vertices.Add(PMIC13);
          MT22B.Vertices.Add(PMITB13);
          MT22B.Vertices.Add(PMITB12);
          MT22B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT22B);
          // Mesh Bottom 11 Back
          Mesh MB11B = new Mesh();
          MB11B.Vertices.Add(PMIC11);
          MB11B.Vertices.Add(PMIB11);
          MB11B.Vertices.Add(PMIC12);
          MB11B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB11B);
          // Mesh Bottom 12 Back
          Mesh MB12B = new Mesh();
          MB12B.Vertices.Add(PMIC12);
          MB12B.Vertices.Add(PMIB11);
          MB12B.Vertices.Add(PMIB12);
          MB12B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB12B);
          // Mesh Bottom 21 Back
          Mesh MB21B = new Mesh();
          MB21B.Vertices.Add(PMIC12);
          MB21B.Vertices.Add(PMIC13);
          MB21B.Vertices.Add(PMIB12);
          MB21B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MB21B);
          // Mesh Bottom 22 Back
          Mesh MB22B = new Mesh();
          MB22B.Vertices.Add(PMIC13);
          MB22B.Vertices.Add(PMIB12);
          MB22B.Vertices.Add(PMIB13);
          MB22B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB22B);

          //
          // Capping Faces
          //

          // Mesh Top 11 Cap Start
          Mesh MT11CS = new Mesh();
          MT11CS.Vertices.Add(PMOC11);
          MT11CS.Vertices.Add(PMIC11);
          MT11CS.Vertices.Add(PMITB11);
          MT11CS.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT11CS);
          // Mesh Bottom 11 Cap Start
          Mesh MB11CS = new Mesh();
          MB11CS.Vertices.Add(PMOC11);
          MB11CS.Vertices.Add(PMIC11);
          MB11CS.Vertices.Add(PMIB11);
          MB11CS.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MB11CS);
          // Mesh Top 22 Cap End
          Mesh MT22CE = new Mesh();
          MT22CE.Vertices.Add(PMOC13);
          MT22CE.Vertices.Add(PMIC13);
          MT22CE.Vertices.Add(PMIT11);
          MT22CE.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MT22CE);
          // Mesh Bottom 22 Cap End
          Mesh MB22CE = new Mesh();
          MB22CE.Vertices.Add(PMOC13);
          MB22CE.Vertices.Add(PMIC13);
          MB22CE.Vertices.Add(PMIB13);
          MB22CE.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB22CE);
        }

        //
        // Combine Mesh and Add to List
        //

        Mesh MJ = new Mesh();
        MJ.Append(lsM_Out);
        MJ.UnifyNormals();
        lsM_Out.Clear();
        lsM_Out.Add(MJ);

        //
        // End Block
        //
        if( (iType == 3) && (iLine == 1) )
        {
          if (bCap == true)
          {
            List<Mesh> lsMF = new List<Mesh>();
            //  Mesh Front Top
            Mesh MF11FT = new Mesh();
            MF11FT.Vertices.Add(PMOCB12);
            MF11FT.Vertices.Add(PMITB11);
            MF11FT.Vertices.Add(PMOCB13);
            MF11FT.Faces.AddFace(0, 1, 2);
            lsMF.Add(MF11FT);
            //  Mesh Front Bottom
            Mesh MF11FB = new Mesh();
            MF11FB.Vertices.Add(PMIB11);
            MF11FB.Vertices.Add(PMOCB12);
            MF11FB.Vertices.Add(PMOCB13);
            MF11FB.Faces.AddFace(0, 1, 2);
            lsMF.Add(MF11FB);
            //  Mesh Rear Top
            Mesh MF11RT = new Mesh();
            MF11RT.Vertices.Add(PMICB12);
            MF11RT.Vertices.Add(PMITB11);
            MF11RT.Vertices.Add(PMICB13);
            MF11RT.Faces.AddFace(0, 2, 1);
            lsMF.Add(MF11RT);
            //  Mesh Rear Bottom
            Mesh MF11RB = new Mesh();
            MF11RB.Vertices.Add(PMIB11);
            MF11RB.Vertices.Add(PMICB12);
            MF11RB.Vertices.Add(PMICB13);
            MF11RB.Faces.AddFace(0, 2, 1);
            lsMF.Add(MF11RB);
            //  Mesh Top Top
            Mesh MF11TT = new Mesh();
            MF11TT.Vertices.Add(PMOC11);
            MF11TT.Vertices.Add(PMITB11);
            MF11TT.Vertices.Add(PMIC11);
            MF11TT.Faces.AddFace(0, 1, 2);
            lsMF.Add(MF11TT);
            //  Mesh Top Bottom
            Mesh MF11TB = new Mesh();
            MF11TB.Vertices.Add(PMIB11);
            MF11TB.Vertices.Add(PMOC11);
            MF11TB.Vertices.Add(PMIC11);
            MF11TB.Faces.AddFace(0, 1, 2);
            lsMF.Add(MF11TB);
            //  Mesh Bottom Front Top
            Mesh MF11BFT = new Mesh();
            MF11BFT.Vertices.Add(PMOCB12);
            MF11BFT.Vertices.Add(PMICB11);
            MF11BFT.Vertices.Add(PMITB11);
            MF11BFT.Faces.AddFace(0, 1, 2);
            lsMF.Add(MF11BFT);
            //  Mesh Bottom Front Bottom
            Mesh MF11BFB = new Mesh();
            MF11BFB.Vertices.Add(PMIB11);
            MF11BFB.Vertices.Add(PMICB11);
            MF11BFB.Vertices.Add(PMOCB12);
            MF11BFB.Faces.AddFace(0, 1, 2);
            lsMF.Add(MF11BFB);
            //  Mesh Bottom Rear Top
            Mesh MF11BRT = new Mesh();
            MF11BRT.Vertices.Add(PMICB12);
            MF11BRT.Vertices.Add(PMICB11);
            MF11BRT.Vertices.Add(PMITB11);
            MF11BRT.Faces.AddFace(0, 2, 1);
            lsMF.Add(MF11BRT);
            //  Mesh Bottom Rear Bottom
            Mesh MF11BRB = new Mesh();
            MF11BRB.Vertices.Add(PMIB11);
            MF11BRB.Vertices.Add(PMICB11);
            MF11BRB.Vertices.Add(PMICB12);
            MF11BRB.Faces.AddFace(0, 2, 1);
            lsMF.Add(MF11BRB);

            //
            // Combine Meshes and Add to List
            //

            Mesh MFJ = new Mesh();
            MFJ.Append(lsMF);
            MFJ.UnifyNormals();
            lsM_Out.Add(MFJ);
            lsMF.Clear();
          }
        }
      }
    }

    else if ( (iType == 5) || (iType == 7) || (iType == 8) || (iType == 12)
      || ((iType == 2) && (iLine + 1 == 2 * iNumV))
      // || ((iType == 3) && (iLine + 1 == 2 * iNumV))
      || ((iType == 6) && (iLine + 1 == 2 * iNumV))
      || ((iType == 15) && (iLine + 1 == 2 * iNumV)) )
    {

      //
      // Top Segments
      //

      if (bTop == true)
      {

        //
        // Line
        //

        // Outer Center
        Point3d P11 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d P12 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d P13 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(P11);
        lsP_Out.Add(P12);
        lsP_Out.Add(P13);
        Line L11 = new Line(P11, P12);
        Line L12 = new Line(P12, P13);
        lsL_Return.Add(L11);
        // lsL_Return.Add(L12);

        //
        // Points
        //

        // Outer Center (Same as Helix Points)
        Point3d PMOC11 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d PMOC12 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d PMOC13 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(PMOC11);
        lsP_Out.Add(PMOC12);
        lsP_Out.Add(PMOC13);
        // Inner Center
        Point3d PMIC11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIC12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIC13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIC11);
        lsP_Out.Add(PMIC12);
        lsP_Out.Add(PMIC13);
        // Inner Top
        Point3d PMIT11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
        Point3d PMIT12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 1.0 + 0.5 * dSpace);
        Point3d PMIT13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIT11);
        lsP_Out.Add(PMIT12);
        lsP_Out.Add(PMIT13);
        // Inner Top Before
        Point3d PMITB11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 2.0 * dAngStpRad), (double) iLine + 1.0 - 1.0 * dSpace);
        Point3d PMITB12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 1.0 * dAngStpRad), (double) iLine + 1.0 - 0.5 * dSpace);
        Point3d PMITB13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 0.0 * dAngStpRad), (double) iLine + 1.0 - 0.0 * dSpace);
        lsP_Out.Add(PMITB11);
        lsP_Out.Add(PMITB12);
        lsP_Out.Add(PMITB13);
        // Inner Bottom
        Point3d PMIB11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine - 1.0 + 0.0 * dSpace);
        Point3d PMIB12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine - 1.0 + 0.5 * dSpace);
        Point3d PMIB13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine - 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIB11);
        lsP_Out.Add(PMIB12);
        lsP_Out.Add(PMIB13);
        // Inner Bottom After
        Point3d PMIBA11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIBA12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 3.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 3.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIBA13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 4.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 4.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIBA11);
        lsP_Out.Add(PMIBA12);
        lsP_Out.Add(PMIBA13);

        //
        // Mesh
        //

        //
        // Front Faces
        //

        // Mesh Top 1
        Mesh MT11 = new Mesh();
        MT11.Vertices.Add(PMOC11);
        MT11.Vertices.Add(PMITB12);
        MT11.Vertices.Add(PMITB11);
        MT11.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MT11);
        // Mesh Top 2
        Mesh MT12 = new Mesh();
        MT12.Vertices.Add(PMOC11);
        MT12.Vertices.Add(PMOC12);
        MT12.Vertices.Add(PMITB12);
        MT12.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MT12);
        // Mesh Bottom 1
        Mesh MB11 = new Mesh();
        MB11.Vertices.Add(PMOC11);
        MB11.Vertices.Add(PMIB11);
        MB11.Vertices.Add(PMIB12);
        MB11.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MB11);
        // Mesh Bottom 2
        Mesh MB12 = new Mesh();
        MB12.Vertices.Add(PMOC11);
        MB12.Vertices.Add(PMIB12);
        MB12.Vertices.Add(PMOC12);
        MB12.Faces.AddFace(0, 2, 1);
        lsM_Out.Add(MB12);

        if (bCap == true)
        {

          //
          // Back Face
          //

          // Mesh Top 1 Back
          Mesh MT11B = new Mesh();
          MT11B.Vertices.Add(PMIC11);
          MT11B.Vertices.Add(PMITB12);
          MT11B.Vertices.Add(PMITB11);
          MT11B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MT11B);
          // Mesh Top 2 Back
          Mesh MT12B = new Mesh();
          MT12B.Vertices.Add(PMIC11);
          MT12B.Vertices.Add(PMITB12);
          MT12B.Vertices.Add(PMIC12);
          MT12B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MT12B);
          // Mesh Bottom 1 Back
          Mesh MB11B = new Mesh();
          MB11B.Vertices.Add(PMIC11);
          MB11B.Vertices.Add(PMIB12);
          MB11B.Vertices.Add(PMIB11);
          MB11B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MB11B);
          // Mesh Bottom 2 Back
          Mesh MB12B = new Mesh();
          MB12B.Vertices.Add(PMIC11);
          MB12B.Vertices.Add(PMIC12);
          MB12B.Vertices.Add(PMIB12);
          MB12B.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MB12B);

          //
          // Capping Faces
          //

          // Mesh Top 1 Side
          Mesh MC11TS = new Mesh();
          MC11TS.Vertices.Add(PMIC11);
          MC11TS.Vertices.Add(PMOC11);
          MC11TS.Vertices.Add(PMITB11);
          MC11TS.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MC11TS);
          // Mesh Bottom 1 Side
          Mesh MC11BS = new Mesh();
          MC11BS.Vertices.Add(PMIC11);
          MC11BS.Vertices.Add(PMIB11);
          MC11BS.Vertices.Add(PMOC11);
          MC11BS.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MC11BS);
          // Mesh Top 2 Side
          Mesh MC12TS = new Mesh();
          MC12TS.Vertices.Add(PMIC12);
          MC12TS.Vertices.Add(PMITB12);
          MC12TS.Vertices.Add(PMOC12);
          MC12TS.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MC12TS);
          // Mesh Bottom 2 Side
          Mesh MC12BS = new Mesh();
          MC12BS.Vertices.Add(PMIC12);
          MC12BS.Vertices.Add(PMOC12);
          MC12BS.Vertices.Add(PMIB12);
          MC12BS.Faces.AddFace(0, 2, 1);
          lsM_Out.Add(MC12BS);

          //
          // Top Intersection Triangle
          //

          if ((iLine + 1 != 2 * iNumV))
          {
            List<Mesh> lsMT = new List<Mesh>();
            // Mesh 1 Side
            Mesh MT11S = new Mesh();
            MT11S.Vertices.Add(PMOC12);
            MT11S.Vertices.Add(PMIC12);
            MT11S.Vertices.Add(PMIT12);
            MT11S.Faces.AddFace(0, 2, 1);
            //lsM_Out.Add(MT11S);
            lsMT.Add(MT11S);
            // Mesh 1 Front Bottom
            Mesh MT11FB = new Mesh();
            MT11FB.Vertices.Add(PMOC12);
            MT11FB.Vertices.Add(PMIT11);
            MT11FB.Vertices.Add(PMITB12);
            MT11FB.Faces.AddFace(0, 2, 1);
            //lsM_Out.Add(MT11FB);
            lsMT.Add(MT11FB);
            // Mesh 1 Front Top
            Mesh MT11FT = new Mesh();
            MT11FT.Vertices.Add(PMOC12);
            MT11FT.Vertices.Add(PMIT12);
            MT11FT.Vertices.Add(PMIT11);
            MT11FT.Faces.AddFace(0, 2, 1);
            //lsM_Out.Add(MT11FT);
            lsMT.Add(MT11FT);
            // Mesh 1 Rear Bottom
            Mesh MT11RB = new Mesh();
            MT11RB.Vertices.Add(PMIC12);
            MT11RB.Vertices.Add(PMITB12);
            MT11RB.Vertices.Add(PMIT11);
            MT11RB.Faces.AddFace(0, 2, 1);
            //lsM_Out.Add(MT11RB);
            lsMT.Add(MT11RB);
            // Mesh 1 Rear Top
            Mesh MT11RT = new Mesh();
            MT11RT.Vertices.Add(PMIC12);
            MT11RT.Vertices.Add(PMIT11);
            MT11RT.Vertices.Add(PMIT12);
            MT11RT.Faces.AddFace(0, 2, 1);
            //lsM_Out.Add(MT11RT);
            lsMT.Add(MT11RT);
            // Mesh 1 Bottom
            Mesh MT11BB = new Mesh();
            MT11BB.Vertices.Add(PMOC12);
            MT11BB.Vertices.Add(PMIC12);
            MT11BB.Vertices.Add(PMITB12);
            MT11BB.Faces.AddFace(0, 2, 1);
            //lsM_Out.Add(MT11BB);

            //
            // Combine Faces to a Mesh and Add to List
            //

            lsMT.Add(MT11BB);
            Mesh MJT = new Mesh();
            MJT.Append(lsMT);
            MJT.UnifyNormals();
            lsM_Out.Add(MJT);
            lsMT.Clear();

          }
        }

        //
        // Combine Meshes and Add to List
        //

        Mesh MJ = new Mesh();
        MJ.Append(lsM_Out);
        //MJ.UnifyNormals();
        lsM_Out.Clear();
        lsM_Out.Add(MJ);

      }
    }

    else if (
      (iType == 4)
      || (iType == 9)
      || (iType == 10)
      || (iType == 14)
      || ((iType == 2) && (iLine == 0))
      || ((iType == 3) && (iLine + 1 == 2 * iNumV))
      )
    {

      //
      // Bottom Segments
      //

      if (bBottom == true)
      {

        //
        // Line
        //

        // Outer Center (Helix Points)
        Point3d P11 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d P12 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d P13 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(P11);
        lsP_Out.Add(P12);
        lsP_Out.Add(P12);
        Line L11 = new Line(P11, P12);
        Line L12 = new Line(P12, P13);
        //lsL_Return.Add(L11);
        lsL_Return.Add(L12);

        //
        // Points
        //

        // Outer Center (Same as Helix Points)
        Point3d PMOC11 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 * dSpace);
        Point3d PMOC12 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.5 * dSpace);
        Point3d PMOC13 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 * dSpace);
        lsP_Out.Add(PMOC11);
        lsP_Out.Add(PMOC12);
        lsP_Out.Add(PMOC13);
        // Inner Center
        Point3d PMIC11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIC12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIC13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIC11);
        lsP_Out.Add(PMIC12);
        lsP_Out.Add(PMIC13);
        // Inner Top
        Point3d PMIT11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
        Point3d PMIT12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 1.0 + 0.5 * dSpace);
        Point3d PMIT13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIT11);
        lsP_Out.Add(PMIT12);
        lsP_Out.Add(PMIT13);
        // Inner Top Before
        Point3d PMITB11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 2.0 * dAngStpRad), (double) iLine + 1.0 - 1.0 * dSpace);
        Point3d PMITB12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 1.0 * dAngStpRad), (double) iLine + 1.0 - 0.5 * dSpace);
        Point3d PMITB13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 0.0 * dAngStpRad), (double) iLine + 1.0 - 0.0 * dSpace);
        lsP_Out.Add(PMITB11);
        lsP_Out.Add(PMITB12);
        lsP_Out.Add(PMITB13);
        // Inner Bottom
        Point3d PMIB11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine - 1.0 + 0.0 * dSpace);
        Point3d PMIB12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine - 1.0 + 0.5 * dSpace);
        Point3d PMIB13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine - 1.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIB11);
        lsP_Out.Add(PMIB12);
        lsP_Out.Add(PMIB13);
        // Inner Bottom After
        Point3d PMIBA11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
        Point3d PMIBA12 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 3.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 3.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
        Point3d PMIBA13 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 4.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 4.0 * dAngStpRad), (double) iLine + 0.0 + 1.0 * dSpace);
        lsP_Out.Add(PMIBA11);
        lsP_Out.Add(PMIBA12);
        lsP_Out.Add(PMIBA13);

        //
        // Mesh
        //

        //
        // Front Faces
        //

        // Mesh Top 1
        Mesh MT11 = new Mesh();
        MT11.Vertices.Add(PMOC12);
        MT11.Vertices.Add(PMOC13);
        MT11.Vertices.Add(PMITB13);
        MT11.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MT11);
        // Mesh Bottom 1
        Mesh MB11 = new Mesh();
        MB11.Vertices.Add(PMOC12);
        MB11.Vertices.Add(PMIB13);
        MB11.Vertices.Add(PMOC13);
        MB11.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB11);
        // Mesh Top 0
        Mesh MT10 = new Mesh();
        MT10.Vertices.Add(PMIC11);
        MT10.Vertices.Add(PMOC12);
        MT10.Vertices.Add(PMIT11);
        MT10.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MT10);
        // Mesh Bottom 0
        Mesh MB10 = new Mesh();
        MB10.Vertices.Add(PMIC11);
        MB10.Vertices.Add(PMIB13);
        MB10.Vertices.Add(PMOC12);
        MB10.Faces.AddFace(0, 1, 2);
        lsM_Out.Add(MB10);


        if (bCap == true)
        {

          //
          // Back Faces
          //

          // Mesh Top 0 Back
          Mesh MT10B = new Mesh();
          MT10B.Vertices.Add(PMIC11);
          MT10B.Vertices.Add(PMIT11);
          MT10B.Vertices.Add(PMIC12);
          MT10B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT10B);
          // Mesh Top 1 Back
          Mesh MT11B = new Mesh();
          MT11B.Vertices.Add(PMIC12);
          MT11B.Vertices.Add(PMIC13);
          MT11B.Vertices.Add(PMIT11);
          MT11B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT11B);
          // Mesh Bottom 0 Back
          Mesh MB10B = new Mesh();
          MB10B.Vertices.Add(PMIC12);
          MB10B.Vertices.Add(PMIB13);
          MB10B.Vertices.Add(PMIC11);
          MB10B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB10B);
          // Mesh Bottom 1 Back
          Mesh MB11B = new Mesh();
          MB11B.Vertices.Add(PMIC12);
          MB11B.Vertices.Add(PMIC13);
          MB11B.Vertices.Add(PMIB13);
          MB11B.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB11B);

          //
          // Capping Faces
          //

          // Mesh Top 1 Side
          Mesh MT11S = new Mesh();
          MT11S.Vertices.Add(PMOC13);
          MT11S.Vertices.Add(PMIC13);
          MT11S.Vertices.Add(PMIT11);
          MT11S.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MT11S);
          // Mesh Bottom 1 Side
          Mesh MB11S = new Mesh();
          MB11S.Vertices.Add(PMOC13);
          MB11S.Vertices.Add(PMIB13);
          MB11S.Vertices.Add(PMIC13);
          MB11S.Faces.AddFace(0, 1, 2);
          lsM_Out.Add(MB11S);

        }

        //
        // Combine Meshes and Add to List
        //

        Mesh MJ = new Mesh();
        MJ.Append(lsM_Out);
        MJ.UnifyNormals();
        lsM_Out.Clear();
        lsM_Out.Add(MJ);

        //
        // Gap Fillling Block
        //

        if ((bCap == true) && (iLine != 0))
        {
          List<Mesh> lsMF = new List<Mesh>();
          //  Mesh Top
          Mesh MF11T = new Mesh();
          MF11T.Vertices.Add(PMOC12);
          MF11T.Vertices.Add(PMIB13);
          MF11T.Vertices.Add(PMIC11);
          MF11T.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF11T);
          //  Mesh Front
          Mesh MF11F = new Mesh();
          MF11F.Vertices.Add(PMOC12);
          MF11F.Vertices.Add(PMIC11);
          MF11F.Vertices.Add(PMIB12);
          MF11F.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF11F);
          //  Mesh Bottom
          Mesh MF11B = new Mesh();
          MF11B.Vertices.Add(PMOC12);
          MF11B.Vertices.Add(PMIB12);
          MF11B.Vertices.Add(PMIB13);
          MF11B.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF11B);
          //  Mesh Rear
          Mesh MF11R = new Mesh();
          MF11R.Vertices.Add(PMIC11);
          MF11R.Vertices.Add(PMIB12);
          MF11R.Vertices.Add(PMIB13);
          MF11R.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF11R);

          //
          // Combine Meshes and Add to List
          //

          Mesh MFJ = new Mesh();
          MFJ.Append(lsMF);
          MFJ.UnifyNormals();
          lsM_Out.Add(MFJ);
          lsMF.Clear();

          //
          // Test Block
          //
          // Mesh Top
          Mesh MF22T = new Mesh();
          MF22T.Vertices.Add(PMOC12);
          MF22T.Vertices.Add(PMITB12);
          MF22T.Vertices.Add(PMITB13);
          MF22T.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF22T);
          // Mesh Bottom
          Mesh MF22B = new Mesh();
          MF22B.Vertices.Add(PMOC12);
          MF22B.Vertices.Add(PMIC11);
          MF22B.Vertices.Add(PMITB12);
          MF22B.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF22B);
          // Mesh Side
          Mesh MF22S = new Mesh();
          MF22S.Vertices.Add(PMOC12);
          MF22S.Vertices.Add(PMITB13);
          MF22S.Vertices.Add(PMIC11);
          MF22S.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF22S);
          // Mesh Rear
          Mesh MF22R = new Mesh();
          MF22R.Vertices.Add(PMIC11);
          MF22R.Vertices.Add(PMITB13);
          MF22R.Vertices.Add(PMITB12);
          MF22R.Faces.AddFace(0, 1, 2);
          lsMF.Add(MF22R);

          //
          // Combine Meshes and Add to List
          //

          Mesh MFT = new Mesh();
          MFT.Append(lsMF);
          MFT.UnifyNormals();
          lsM_Out.Add(MFT);
          lsMF.Clear();

        }
      }
    }

    //
    // Output
    //

    lsM = lsM_Out;
    lsP = lsP_Out;
    return lsL_Return;

  }

  //
  // Support Left
  //
  List<Line> lsL_SupportLeft(int iLine, int iColumn, int iType, int iNumV, double dAngStpRad, double dR1, double dR2, double dSpace) // iNumV = iNumV
  {
    List<Line> lsL_Return = new List<Line>();
    double dAngPosRad = -((double) iColumn + 1.0) * 2.0 * dAngStpRad;

    // Middle Segments
    if ( (iType == 4) || (iType == 5) || (iType == 6) || (iType == 7)
      || (iType == 10) || (iType == 11) || (iType == 15)
      || ((iType == 1) && (iLine != 0)) || ((iType == 1) && (iLine + 1 == 2 * iNumV)) )
    {

      // Helix Pt 1 Middle 1
      Point3d P11 = new Point3d(-dR1 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P12 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);

      // Helix Pt 1 Top 1
      Point3d P21 = new Point3d(-dR1 * Math.Sin(dAngPosRad - 2.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad - 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P22 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);

      // Helix Pt 1 Bottom 1
      Point3d P31 = new Point3d(-dR1 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine - 1.0 + 0.0 * dSpace);
      Point3d P32 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);

      // Helix Pt 1 Top 2
      Point3d P41 = new Point3d(-dR1 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
      Point3d P42 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);

      // Helix Pt 1 Bottom 2
      Point3d P51 = new Point3d(-dR1 * Math.Sin(dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P52 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);

      // Helix Pt 2 Top 1
      Point3d P61 = new Point3d(-dR1 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
      Point3d P62 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      // Helix Pt 2 Bottom 1
      Point3d P71 = new Point3d(-dR1 * Math.Sin(dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P72 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      Line L11 = new Line(P11, P12);
      Line L21 = new Line(P21, P22);
      Line L31 = new Line(P31, P32);
      Line L41 = new Line(P41, P42);
      Line L51 = new Line(P51, P52);
      Line L61 = new Line(P61, P62);
      Line L71 = new Line(P71, P72);
      lsL_Return.Add(L11);
      lsL_Return.Add(L21);
      lsL_Return.Add(L31);
      lsL_Return.Add(L41);
      lsL_Return.Add(L51);
      lsL_Return.Add(L61);
      lsL_Return.Add(L71);

    }

      // Top End
    else if ( (iType == 6) || (iType == 7) || (iType == 10) || (iType == 11)
      || ((iType == 1) && ((iLine + 1) == (2 * iNumV))) )
    {

      // Helix Pt 2 Middle 1
      Point3d P11 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);
      Point3d P12 = new Point3d(-dR1 * Math.Sin(dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);

      Line L11 = new Line(P11, P12);
      lsL_Return.Add(L11);

    }


      // Bottom End
    else if ( ((iType == 1) && (iLine == 0))
      || (iType == 3) || (iType == 8) || (iType == 9) || (iType == 13) )
    {
      // Helix Pt 1 Top 1
      Point3d P11 = new Point3d(-dR1 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
      Point3d P12 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      // Helix Pt 1 Bottom 1
      Point3d P22 = new Point3d(-dR1 * Math.Sin(dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P21 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      // Helix Pt 1 Middle 0
      Point3d P32 = new Point3d(-dR1 * Math.Sin(dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P31 = new Point3d(-dR2 * Math.Sin(dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      Line L11 = new Line(P11, P12);
      Line L21 = new Line(P21, P22);
      Line L31 = new Line(P31, P32);
      lsL_Return.Add(L11);
      lsL_Return.Add(L21);
      lsL_Return.Add(L31);

    }

    return lsL_Return;
  }

  //
  // Support Right
  //
  List<Line> lsL_SupportRight(int iLine, int iColumn, int iType, int iNumV, double dAngStpRad, double dR1, double dR2, double dSpace) // iNumV = iNumV
  {
    List<Line> lsL_Return = new List<Line>();
    double dAngPosRad = ((double) iColumn + 0.0) * 2.0 * dAngStpRad;

    // Middle Segments
    if ( (iType == 1) || (iType == 3) || (iType == 5) || (iType == 6) || (iType == 7) || (iType == 8) || (iType == 12) || (iType == 15)
      || ((iType == 2) && (iLine != 0))
      || ((iType == 2) && (iLine + 1 == 2 * iNumV)) )
    {
      // Helix Pt 1 Middle 1
      Point3d P11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P12 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);

      // Helix Pt 1 Bottom 1
      Point3d P21 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine - 1.0 + 0.0 * dSpace);
      Point3d P22 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);

      // Helix Pt 1 Bottom 2
      Point3d P31 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P32 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);

      // Helix PT 1 Top 1
      Point3d P41 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad - 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad - 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P42 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);

      // Helix Pt 1 Top 2
      Point3d P51 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
      Point3d P52 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);

      // Helix Pt 2 Bottom 1
      Point3d P61 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P62 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      // Helix Pt 2 Top 2
      Point3d P71 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
      Point3d P72 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      Line L11 = new Line(P11, P12); // 0
      Line L21 = new Line(P21, P22); // 1
      Line L31 = new Line(P31, P32); // 2
      Line L41 = new Line(P41, P42); // 3
      Line L51 = new Line(P51, P52); // 4
      Line L61 = new Line(P61, P62); // 5
      Line L71 = new Line(P71, P72); // 6
      lsL_Return.Add(L11);
      lsL_Return.Add(L21);
      lsL_Return.Add(L31);
      lsL_Return.Add(L41);
      lsL_Return.Add(L51);
      lsL_Return.Add(L61);
      lsL_Return.Add(L71);
    }

      // Top End
    else if ( ((iType == 2) && ((iLine + 1) == (2 * iNumV)))
      || (iType == 5) || (iType == 7) || (iType == 8) || (iType == 12) )
    {
      // Helix Pt 2 Middle 1
      Point3d P11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
      Point3d P12 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      Line L11 = new Line(P11, P12);
      lsL_Return.Add(L11);
    }

    // Bottom End
    if ( ( (iType == 2) && (iLine == 0) )
      || (iType == 4) || (iType == 9) || (iType == 10) || (iType == 14) )
    {
      // Helix Pt 1 Top 1
      Point3d P11 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 1.0 + 0.0 * dSpace);
      Point3d P12 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      // Helix Pt 1 Bottom 1
      Point3d P21 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 2.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 2.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P22 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      // Helix Pt 1 Middle 0
      Point3d P31 = new Point3d(dR1 * Math.Sin(1.0 * dAngPosRad + 0.0 * dAngStpRad), dR1 * Math.Cos(1.0 * dAngPosRad + 0.0 * dAngStpRad), (double) iLine + 0.0 + 0.0 * dSpace);
      Point3d P32 = new Point3d(dR2 * Math.Sin(1.0 * dAngPosRad + 1.0 * dAngStpRad), dR2 * Math.Cos(1.0 * dAngPosRad + 1.0 * dAngStpRad), (double) iLine + 0.0 + 0.5 * dSpace);

      Line L11 = new Line(P11, P12);
      Line L21 = new Line(P21, P22);
      Line L31 = new Line(P31, P32);
      lsL_Return.Add(L11);
      lsL_Return.Add(L21);
      lsL_Return.Add(L31);
    }

    return lsL_Return;
  }
  // </Custom additional code> 
}