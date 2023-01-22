
using UnityEngine;
using UnityEditor;
#if UNITY_2019_1_OR_NEWER
using UnityEditor.ShortcutManagement;
#endif

namespace Technie.PhysicsCreator
{
#if UNITY_2019_1_OR_NEWER
	// NB: Lots of shortchuts have to be registered as a global shortcut (no context type specified) because if we register
	// a context it only fires when the window has direct focus, which often isn't the case as the user is painting
	// (technically the scene view has focus, even though the hull painter window is the selected window)
	public class Shortcuts
	{
		[Shortcut("Collider Creator/Add Hull", KeyCode.Q, ShortcutModifiers.Shift)]
		public static void AddHull()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.AddHull();
				HullPainterWindow.instance.Repaint();
			}
		}

		[Shortcut("Collider Creator/Generate colliders", KeyCode.W, ShortcutModifiers.Shift)]
		public static void Generate()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.GenerateColliders();
			}
		}

		[Shortcut("Collider Creator/Delete generated colliders", KeyCode.E, ShortcutModifiers.Shift)]
		public static void DeleteGenerated()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.DeleteGenerated();
				HullPainterWindow.instance.Repaint();
			}
		}

		[Shortcut("Collider Creator/Stop painting", KeyCode.S, ShortcutModifiers.Shift)]
		public static void StopPainting()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.StopPainting();
				HullPainterWindow.instance.Repaint();
			}
		}


		[Shortcut("Collider Creator/Paint all faces", KeyCode.A, ShortcutModifiers.Shift)]
		public static void PaintAll()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.PaintAllFaces();
			}
		}

		[Shortcut("Collider Creator/Unpaint all faces", KeyCode.Z, ShortcutModifiers.Shift)]
		public static void UnpaintAllFaces()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.UnpaintAllFaces();
			}
		}

		[Shortcut("Collider Creator/Cycle brush size", KeyCode.Alpha1, ShortcutModifiers.Shift)]
		public static void AdvanceBrushSize()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.AdvanceBrushSize();
				HullPainterWindow.instance.Repaint();
			}
		}

		[Shortcut("Collider Creator/Select Pipette", KeyCode.BackQuote, ShortcutModifiers.Shift)]
		public static void SelectPipette()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.SelectPipette();
				HullPainterWindow.instance.Repaint();
			}
		}

		[Shortcut("Collider Creator/Cycle hull type", KeyCode.T, ShortcutModifiers.Shift)]
		public static void CycleHullType()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.CycleHullType();
				HullPainterWindow.instance.Repaint();
			}
		}

		[Shortcut("Collider Creator/Select next hull", KeyCode.Equals, ShortcutModifiers.Shift)]
		public static void SelectNextHull()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.AdvanceSelectedHull(1);
				HullPainterWindow.instance.Repaint();
			}
		}

		[Shortcut("Collider Creator/Select previous hull", KeyCode.Minus, ShortcutModifiers.Shift)]
		public static void SelectPrevHull()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.AdvanceSelectedHull(-1);
				HullPainterWindow.instance.Repaint();
			}
		}

		[Shortcut("Collider Creator/Toggle as child", KeyCode.Y, ShortcutModifiers.Shift)]
		public static void ToggleIsChild()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.ToggleIsChild();
				HullPainterWindow.instance.Repaint();
			}
		}

		[Shortcut("Collider Creator/Toggle is trigger", KeyCode.U, ShortcutModifiers.Shift)]
		public static void ToggleIsTrigger()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.ToggleIsTrigger();
				HullPainterWindow.instance.Repaint();
			}
		}

		[Shortcut("Collider Creator/Delete active hull", KeyCode.Backspace, ShortcutModifiers.Shift)]
		public static void DeleteActiveHull()
		{
			if (HullPainterWindow.IsOpen())
			{
				HullPainterWindow.instance.DeleteActiveHull();
				HullPainterWindow.instance.Repaint();
			}
		}

	} // ssalc Shortcuts
#endif // UNITY_2019_1_OR_NEWER
} // ecapseman Technie.PhysicsCreator
