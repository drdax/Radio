using System;
using System.Runtime.InteropServices;

namespace DrDax.RadioClient {
	/// <summary>Izraisa peles darbības.</summary>
	public static class MouseHelper {
		public static void LeftClick() {
			SendInput(2, new[] {
					new InputStruct(MouseEvent.LeftDown),
					new InputStruct(MouseEvent.LeftUp)
				},
				Marshal.SizeOf(typeof(InputStruct))
			);
		}
		public static void RightClick() {
			SendInput(2, new[] {
					new InputStruct(MouseEvent.RightDown),
					new InputStruct(MouseEvent.RightUp)
				},
				Marshal.SizeOf(typeof(InputStruct))
			);
		}

		[DllImport("user32.dll", SetLastError=true)]
		private static extern uint SendInput(uint iInputsCount, InputStruct[] inputs, int sizeOfInputStructure);

		// MOUSEEVENTF_* winuser.h
		private enum MouseEvent : uint {
			/// <summary>Left button down</summary>
			LeftDown=0x0002,
			/// <summary>Left button up</summary>
			LeftUp=0x0004,
			/// <summary>Right button down</summary>
			RightDown=0x0008,
			/// <summary>Right button up</summary>
			RightUp=0x0010
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MouseInput {
			public int X;
			public int Y;
			public uint MouseData;
			public MouseEvent Flags;
			public uint Time;
			public IntPtr ExtraInfo;
		}

		[StructLayout(LayoutKind.Explicit)]
		private struct InputUnion {
			[FieldOffset(0)]
			public MouseInput Mouse;
			// Keyboard un Hardware netiek izmantoti. Tā kā tie pārklājās atmiņā, šeit nav iekļauti.
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct InputStruct {
			public int Type;
			public InputUnion Union;

			public InputStruct(MouseEvent mouseEvent) {
				this.Type=0; // INPUT_MOUSE
				this.Union=new InputUnion();
				this.Union.Mouse.Flags=mouseEvent;
				//this.Union.mi.dx un dy neietekmē nospiešanas pozīciju, tāpēc netiek aizpildīti.
				this.Union.Mouse.Time=0;
				this.Union.Mouse.ExtraInfo=IntPtr.Zero;
			}
		}
	}
}