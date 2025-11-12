using A19CPU.Tests.Records;

namespace A19CPU.Tests;

public partial class CommandTests
{
	#region Simple CPU commands

	public class SimpleCommandTestDataProvider
		: TheoryData<byte[], CpuState>
	{
		public SimpleCommandTestDataProvider()
		{
			//	Disable Interrupts as only code
			Add([0x01], new DebugCpuState() { InterruptsEnabled = false, IP = 0x01 });
			//	NOP as only code
			Add([0x02], new DebugCpuState() { IP = 0x01 });
			//	Suspend as only code
			Add([0x08], new DebugCpuState() { IP = 0x01 });
			//	Enable Interrupts as only code
			Add([0x09], new DebugCpuState() { InterruptsEnabled = true, IP = 0x01 });
			//	Load a,a as only code
			Add([0x20], new DebugCpuState() { IP = 0x01, A = 0, Zero = true, NoCarry = true, ParityEven = true });
			//	add a,a as only code
			Add([0x40], new DebugCpuState() { IP = 0x01, A = 0, Zero = true, NoCarry = true, ParityEven = true });
			//	sub a,a as only code
			Add([0x60], new DebugCpuState() { IP = 0x01, A = 0, Zero = true, NoCarry = true, ParityEven = true });
			//	and a as only code
			Add([0x80], new DebugCpuState() { IP = 0x01, A = 0, Zero = true, NoCarry = true, ParityEven = true });
			//	or a as only code
			Add([0x88], new DebugCpuState() { IP = 0x01, A = 0, Zero = true, NoCarry = true, ParityEven = true });
			//	xor a as only code
			Add([0x90], new DebugCpuState() { IP = 0x01, A = 0, Zero = true, NoCarry = true, ParityEven = true });
			//	cmp a as only code
			Add([0x98], new DebugCpuState() { IP = 0x01, A = 0, Zero = true, NoCarry = true, ParityEven = true });
			//	inc a as only code
			Add([0xA0], new DebugCpuState() { IP = 0x01, A = 1, NonZero = true, NoCarry = true, ParityOdd = true });
			//	inc b as only code
			Add([0xA1], new DebugCpuState() { IP = 0x01, B = 1, NoCarry = true });
			//	inc c as only code
			Add([0xA2], new DebugCpuState() { IP = 0x01, C = 1, NoCarry = true });
			//	dec a as only code
			Add([0xA8], new DebugCpuState() { IP = 0x01, A = 255, NonZero = true, Minus = true, ParityEven = true });
			//	dec b as only code
			Add([0xA9], new DebugCpuState() { IP = 0x01, B = 255, Minus = true });
			//	dec c as only code
			Add([0xAA], new DebugCpuState() { IP = 0x01, C = 255, Minus = true });
			//	set zero flag
			Add([0xE0], new DebugCpuState() { IP = 0x01, Zero = true });
			//	set carry flag
			Add([0xE1], new DebugCpuState() { IP = 0x01, Carry = true });
			//	set Parity Even flag
			Add([0xE2], new DebugCpuState() { IP = 0x01, ParityEven = true });
			//	set Minus flag
			Add([0xE3], new DebugCpuState() { IP = 0x01, Minus = true });
			//	set, then clear zero flag
			Add([0xE0, 0xE4], new DebugCpuState() { IP = 0x02 });
			//	set, then clear carry flag
			Add([0xE1, 0xE5], new DebugCpuState() { IP = 0x02 });
			//	set, then clear Parity Even flag
			Add([0xE2, 0xE6], new DebugCpuState() { IP = 0x02 });
			//	set, then clear Minus flag
			Add([0xE3, 0xE7], new DebugCpuState() { IP = 0x02 });
			//	Set all flags, reset zero flag
			Add([0xE0, 0xE1, 0xE2, 0xE3, 0xE4], new DebugCpuState() { IP = 0x04, Carry = true, ParityEven = true, Minus = true});
			//	Set all flags, reset carry flag
			Add([0xE0, 0xE1, 0xE2, 0xE3, 0xE5], new DebugCpuState() { IP = 0x04, Zero = true, ParityEven = true, Minus = true});
			//	Set all flags, reset parity flag
			Add([0xE0, 0xE1, 0xE2, 0xE3, 0xE6], new DebugCpuState() { IP = 0x04, Zero = true, Carry = true, Minus = true});
			//	Set all flags, reset minus flag
			Add([0xE0, 0xE1, 0xE2, 0xE3, 0xE7], new DebugCpuState() { IP = 0x04, Zero = true, ParityEven = true, Carry = true});
		}
	}

	#endregion

	#region Tests for verifying load command behaviour

	public class LoadCommandTestDataProvider
		: TheoryData<MachineMemory, CpuState>
	{
		public LoadCommandTestDataProvider()
		{
			LoadAccumulatorTests();
			LoadRegisterBTests();
			LoadRegisterCTests();
			LoadDirectNTests();
		}

		/// <summary>
		/// Tests to represent loading values into the accumulator register
		/// </summary>
		private void LoadAccumulatorTests()
		{
			var ldaa = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x20,	//	ld a,a
				}
			};
			ldaa.CopyProgramToExpected();
			Add(ldaa, new DebugCpuState() { A = 0, IP = 0x01, NonZero = true, NoCarry = true, ParityEven = true });
			
			var ldab = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	lb b,n
					[0x01] = 0xBB,	//	187 / 0xBB NZ, NC, PE
					[0x02] = 0x21,	//	ld a,b
				}
			};
			ldab.CopyProgramToExpected();
			Add(ldab, new DebugCpuState() { A = 0xBB, B = 0xBB, IP = 0x03, NonZero = true, NoCarry = true, ParityEven = true });

			var ldac = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	lb c,n
					[0x01] = 0xCC,	//	204 / 0xCC NZ, NC, PE
					[0x02] = 0x22,	//	ld a,c
				}
			};
			ldac.CopyProgramToExpected();
			Add(ldac, new DebugCpuState() { A = 0xCC, C = 0xCC, IP = 0x03, NonZero = true, NoCarry = true, ParityEven = true });
			
			var ldad = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	lb a,n
					[0x01] = 0xDD,	//	221 / 0xDD NZ, NC, PE
				}
			};
			ldad.CopyProgramToExpected();
			Add(ldad, new DebugCpuState() { A = 0xDD, IP = 0x02, NonZero = true, NoCarry = true, ParityEven = true });
			
			var ldadb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	lb b,n
					[0x01] = 0x20,	//	
					[0x02] = 0x24,	//	ld a,[b]
					[0x20] = 0xDB,
				}
			};
			ldadb.CopyProgramToExpected();
			Add(ldadb, new DebugCpuState() { A = 0xDB, B = 0x20, IP = 0x03, NonZero = true, NoCarry = true, ParityEven = true });
			
			var ldadc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	lb c,n
					[0x01] = 0x20,	//	
					[0x02] = 0x25,	//	ld a,[c]
					[0x20] = 0xDC,
				}
			};
			ldadc.CopyProgramToExpected();
			Add(ldadc, new DebugCpuState() { A = 0xDC, C = 0x20, IP = 0x03, NonZero = true, NoCarry = true, ParityOdd = true });
			
			var ldadn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x26,	//	lb a,[n]
					[0x01] = 0x20,	//	
					[0x20] = 0xD1,
				}
			};
			ldadn.CopyProgramToExpected();
			Add(ldadn, new DebugCpuState() { A = 0xD1, IP = 0x02, NonZero = true, NoCarry = true, ParityEven = true });
		}
				
		/// <summary>
		/// Tests to represent loading values into the B register
		/// </summary>
		private void LoadRegisterBTests()
		{
			var ldba = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0xAA,
					[0x02] = 0x28,	//	ld b,a
				}
			};
			ldba.CopyProgramToExpected();
			Add(ldba, new DebugCpuState() { A = 0xAA, B = 0xAA, IP = 0x03, NonZero = true, NoCarry = true, ParityEven = true });
			
			var ldbb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x29,	//	lb b,b
				}
			};
			ldbb.CopyProgramToExpected();
			Add(ldbb, new DebugCpuState() { IP = 0x01, NoCarry = true });

			var ldbc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	lb c,n
					[0x01] = 0xCC,	//	204 / 0xCC NZ, NC, PE
					[0x02] = 0x2A,	//	ld b,c
				}
			};
			ldbc.CopyProgramToExpected();
			Add(ldbc, new DebugCpuState() { B = 0xCC, C = 0xCC, IP = 0x03, NoCarry = true });
			
			var ldbd = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	lb b,n
					[0x01] = 0xDD,	//	221 / 0xDD NZ, NC, PE
				}
			};
			ldbd.CopyProgramToExpected();
			Add(ldbd, new DebugCpuState() { B = 0xDD, IP = 0x02, NoCarry = true });
			
			var ldbdb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	lb b,n
					[0x01] = 0x20,	//	
					[0x02] = 0x2C,	//	ld b,[b]
					[0x20] = 0xDB,
				}
			};
			ldbdb.CopyProgramToExpected();
			Add(ldbdb, new DebugCpuState() { B = 0xDB, IP = 0x03, NoCarry = true });
			
			var ldbdc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	lb c,n
					[0x01] = 0x20,	//	
					[0x02] = 0x2D,	//	ld ,[c]
					[0x20] = 0xDC,
				}
			};
			ldbdc.CopyProgramToExpected();
			Add(ldbdc, new DebugCpuState() { B = 0xDC, C = 0x20, IP = 0x03, NoCarry = true });
			
			var ldbdn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2E,	//	lb b,[n]
					[0x01] = 0x20,	//	
					[0x20] = 0xD2,
				}
			};
			ldbdn.CopyProgramToExpected();
			Add(ldbdn, new DebugCpuState() { B = 0xD2, IP = 0x02, NoCarry = true });
		}

		/// <summary>
		/// Tests to represent loading values into the C register
		/// </summary>
		private void LoadRegisterCTests()
		{
			var ldca = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0xAA,
					[0x02] = 0x30,	//	ld c,a
				}
			};
			ldca.CopyProgramToExpected();
			Add(ldca, new DebugCpuState() { A = 0xAA, C = 0xAA, IP = 0x03, NonZero = true, NoCarry = true, ParityEven = true });
			
			var ldcb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0xBB,
					[0x02] = 0x31,	//	ld c,b
				}
			};
			ldcb.CopyProgramToExpected();
			Add(ldcb, new DebugCpuState() { B = 0xBB, C = 0xBB, IP = 0x03, NoCarry = true });

			var ldcc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x32,
				}
			};
			ldcc.CopyProgramToExpected();
			Add(ldcc, new DebugCpuState() { IP = 0x01, NoCarry = true });
			
			var ldcn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	lb c,n
					[0x01] = 0xDD,	//	221 / 0xDD NZ, NC, PE
				}
			};
			ldcn.CopyProgramToExpected();
			Add(ldcn, new DebugCpuState() { C = 0xDD, IP = 0x02, NoCarry = true });
			
			var ldcdb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	lb b,n
					[0x01] = 0x20,	//	
					[0x02] = 0x34,	//	ld c,[b]
					[0x20] = 0xDB,
				}
			};
			ldcdb.CopyProgramToExpected();
			Add(ldcdb, new DebugCpuState() { B = 0x20, C = 0xDB, IP = 0x03, NoCarry = true });
			
			var ldcdc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	lb c,n
					[0x01] = 0x20,	//	
					[0x02] = 0x35,	//	ld c,[c]
					[0x20] = 0xDC,
				}
			};
			ldcdc.CopyProgramToExpected();
			Add(ldcdc, new DebugCpuState() { C = 0xDC, IP = 0x03, NoCarry = true });
			
			var ldcdn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	lb c,[n]
					[0x01] = 0x20,	//	
					[0x20] = 0xD2,
				}
			};
			ldcdn.CopyProgramToExpected();
			Add(ldcdn, new DebugCpuState() { C = 0xD2, IP = 0x02, NoCarry = true });
		}

		/// <summary>
		/// Tests to represent loading values into the C register
		/// </summary>
		private void LoadDirectNTests()
		{
			var lddna = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0xAA,
					[0x02] = 0x38,	//	ld [n],a
					[0x03] = 0x20,
				}
			};
			lddna.CopyProgramToExpected();
			lddna.Expected[0x20] = 0xAA;
			Add(lddna, new DebugCpuState() { A = 0xAA, IP = 0x04, NonZero = true, NoCarry = true, ParityEven = true });
			
			var lddnb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0xBB,
					[0x02] = 0x39,	//	ld [n],b
					[0x03] = 0x20,
				}
			};
			lddnb.CopyProgramToExpected();
			lddna.Expected[0x20] = 0xBB;
			Add(lddnb, new DebugCpuState() { B = 0xBB, IP = 0x04, NoCarry = true });

			var lddnc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0xCC,
					[0x02] = 0x3A,	//	ld [n],c
					[0x03] = 0x20,
				}
			};
			lddnc.CopyProgramToExpected();
			lddna.Expected[0x20] = 0xCC;
			Add(lddnc, new DebugCpuState() { C = 0xCC, IP = 0x04, NoCarry = true });
			
			var lddnn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x3B,	//	lb [n],n
					[0x01] = 0x20,
					[0x02] = 0xD0,
				}
			};
			lddnn.CopyProgramToExpected();
			lddna.Expected[0x20] = 0xD0;
			Add(lddnn, new DebugCpuState() { IP = 0x03 });
			
			var lddndb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	lb b,n
					[0x01] = 0x20,	//  	
					[0x02] = 0x3C,	//	ld [n],[b]
					[0x03] = 0x21,
					[0x20] = 0xDB,
					[0x21] = 0x40,
					[0x40] = 0xFF,	//	Force a value here, which is where ld [n],[b] should write 0xDB
				}
			};
			lddndb.CopyProgramToExpected();
			lddndb.Expected[0x40] = 0xDB;
			Add(lddndb, new DebugCpuState() { B = 0x20, IP = 0x04, NoCarry = true });
			
			var lddndc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	lb c,n
					[0x01] = 0x20,	//	
					[0x02] = 0x3D,	//	ld [n],[c]
					[0x03] = 0x40,
					[0x20] = 0xDC,
					[0x40] = 0xFF,	//	Force a value here, which is where ld [n],[c] should write 0xDC
				}
			};
			lddndc.CopyProgramToExpected();
			Add(lddndc, new DebugCpuState() { C = 0x20, IP = 0x04, NoCarry = true });
			
			var lddndn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x3E,	//	lb [n],[n]
					[0x01] = 0x20,	//
					[0x02] = 0x40,
					[0x20] = 0xDD,
					[0x40] = 0xFF,	//	Force a value here, which is where ld [n],[n] should write 0xDD
				}
			};
			lddndn.CopyProgramToExpected();
			lddndn.Expected[0x40] = 0xDD;
			Add(lddndn, new DebugCpuState() { IP = 0x03 });
		}
	}

	#endregion
	
	#region Tests for verifying Add command behaviour

	public class AddCommandTestDataProvider
		: TheoryData<MachineMemory, CpuState>
	{
		public AddCommandTestDataProvider()
		{
			AddAccumulatorTests();
			AddRegisterBTests();
			AddRegisterCTests();
			AddDirectNTests();
		}

		/// <summary>
		/// Tests to represent adding values into the accumulator register
		/// </summary>
		private void AddAccumulatorTests()
		{
			var addaa = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x40,	//	add a,a
				}
			};
			addaa.CopyProgramToExpected();
			Add(addaa, new DebugCpuState() { A = 0, IP = 0x01, Zero = true, ParityEven = true });

			var addaa2 = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 1,		//	n=>1
					[0x02] = 0x40,	//	add a,a
				}
			};
			addaa2.CopyProgramToExpected();
			Add(addaa2, new DebugCpuState() { A = 2, IP = 0x03, NonZero = true, NoCarry = true, ParityOdd = true });
			
			var addab = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0x0B,	//	11 / 0x0B NZ, NC, PO
					[0x02] = 0x41,	//	add a,b
				}
			};
			addab.CopyProgramToExpected();
			Add(addab, new DebugCpuState() { A = 0x0B, B = 0x0B, IP = 0x03, NonZero = true, NoCarry = true, ParityOdd = true });

			var addac = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	add c,n
					[0x01] = 0x0C,	//	12 / 0x0C NZ, NC, PE
					[0x02] = 0x42,	//	add a,c
				}
			};
			addac.CopyProgramToExpected();
			Add(addac, new DebugCpuState() { A = 0x0C, C = 0x0C, IP = 0x03, NonZero = true, NoCarry = true, ParityEven = true });
			
			var addad = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x43,	//	add a,n
					[0x01] = 0x0D,	//	13 / 0x0D NZ, NC, PO
				}
			};
			addad.CopyProgramToExpected();
			Add(addad, new DebugCpuState() { A = 0x0D, IP = 0x02, NonZero = true, NoCarry = true, ParityEven = true });
			
			var addadb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0x20,	//	addr 0x20
					[0x02] = 0x44,	//	add a,[b]
					[0x20] = 0xDB,
				}
			};
			addadb.CopyProgramToExpected();
			Add(addadb, new DebugCpuState() { A = 0xDB, B = 0x20, IP = 0x03, NonZero = true, NoCarry = true, ParityEven = true });
			
			var addadc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	add c,n
					[0x01] = 0x20,	//	
					[0x02] = 0x45,	//	add a,[c]
					[0x20] = 0xDC,
				}
			};
			addadc.CopyProgramToExpected();
			Add(addadc, new DebugCpuState() { A = 0xDC, C = 0x20, IP = 0x03, NonZero = true, NoCarry = true, ParityOdd = true });
			
			var addadn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x46,	//	add a,[n]
					[0x01] = 0x20,	//	
					[0x20] = 0xD1,
				}
			};
			addadn.CopyProgramToExpected();
			Add(addadn, new DebugCpuState() { A = 0xD1, IP = 0x02, NonZero = true, NoCarry = true, ParityEven = true });
		}
				
		/// <summary>
		/// Tests to represent adding values into the B register
		/// </summary>
		private void AddRegisterBTests()
		{
			var addba = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	add a,n
					[0x01] = 0x0A,
					[0x02] = 0x48,	//	add b,a
				}
			};
			addba.CopyProgramToExpected();
			Add(addba, new DebugCpuState() { A = 0x0A, B = 0x0A, IP = 0x03, NonZero = true, NoCarry = true, ParityEven = true });
			
			var addbb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x49,	//	add b,b
				}
			};
			addbb.CopyProgramToExpected();
			Add(addbb, new DebugCpuState() { IP = 0x01, NoCarry = true });

			var addbb2 = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b, n
					[0x01] = 0x0B,	//	n=> 0x0B
					[0x02] = 0x49,	//	add b,b
				}
			};
			addbb2.CopyProgramToExpected();
			Add(addbb2, new DebugCpuState() { B = 0x16, IP = 0x03, NoCarry = true });

			var addbc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	add c,n
					[0x01] = 0x0C,	//	12 / 0x0C NZ, NC, PE
					[0x02] = 0x4A,	//	add b,c
				}
			};
			addbc.CopyProgramToExpected();
			Add(addbc, new DebugCpuState() { B = 0x0C, C = 0x0C, IP = 0x03, NoCarry = true });
			
			var addbd = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x4B,	//	add b,n
					[0x01] = 0xDD,	//	221 / 0xDD NZ, NC, PE
				}
			};
			addbd.CopyProgramToExpected();
			Add(addbd, new DebugCpuState() { B = 0xDD, IP = 0x02, NoCarry = true });
			
			var addbdb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0xB0,	//	
					[0x02] = 0x4C,	//	add b,[b]
					[0xB0] = 0x0B,
				}
			};
			addbdb.CopyProgramToExpected();
			Add(addbdb, new DebugCpuState() { B = 0xBB, IP = 0x03, NoCarry = true });
			
			var addbdc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	add c,n
					[0x01] = 0x20,	//	
					[0x02] = 0x4D,	//	add b,[c]
					[0x20] = 0x0C,
				}
			};
			addbdc.CopyProgramToExpected();
			Add(addbdc, new DebugCpuState() { B = 0x0C, C = 0x20, IP = 0x03, NoCarry = true });
			
			var addbdn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x4E,	//	add b,[n]
					[0x01] = 0x20,	//	
					[0x20] = 0xB2,
				}
			};
			addbdn.CopyProgramToExpected();
			Add(addbdn, new DebugCpuState() { B = 0xB2, IP = 0x02, NoCarry = true });
		}

		/// <summary>
		/// Tests to represent adding values into the C register
		/// </summary>
		private void AddRegisterCTests()
		{
			var addca = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0xAA,
					[0x02] = 0x50,	//	add c,a
				}
			};
			addca.CopyProgramToExpected();
			Add(addca, new DebugCpuState() { A = 0xAA, C = 0xAA, IP = 0x03, NonZero = true, NoCarry = true, ParityEven = true });
			
			var addcb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0xBB,
					[0x02] = 0x51,	//	add c,b
				}
			};
			addcb.CopyProgramToExpected();
			Add(addcb, new DebugCpuState() { B = 0xBB, C = 0xBB, IP = 0x03, NoCarry = true });

			var addcc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x52,
				}
			};
			addcc.CopyProgramToExpected();
			Add(addcc, new DebugCpuState() { IP = 0x01, NoCarry = true });
			
			var addcc2 = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0x0C,	//	n=>0x0C
					[0x02] = 0x52,	//	add c,c
				}
			};
			addcc2.CopyProgramToExpected();
			Add(addcc2, new DebugCpuState() { C = 0x18, IP = 0x03, NoCarry = true });
			
			var addcn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0xD0,
					[0x02] = 0x53,	//	add c,n
					[0x03] = 0x0F,
				}
			};
			addcn.CopyProgramToExpected();
			Add(addcn, new DebugCpuState() { C = 0xDF, IP = 0x04, NoCarry = true });
			
			var addcdb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0x20,	//	
					[0x02] = 0x33,	//	ld c,n
					[0x03] = 0xD0,	//	208 / 0xD0 NZ, NC, PO
					[0x04] = 0x54,	//	add c,[b] => 0xDB
					[0x20] = 0x0B,
				}
			};
			addcdb.CopyProgramToExpected();
			Add(addcdb, new DebugCpuState() { B = 0x20, C = 0xDB, IP = 0x05, NoCarry = true });
			
			var addcdc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0x20,
					[0x02] = 0x55,	//	add c,[c] => 0xCC
					[0x20] = 0xAC,
				}
			};
			addcdc.CopyProgramToExpected();
			Add(addcdc, new DebugCpuState() { C = 0xCC, IP = 0x03, NoCarry = true });
			
			var addcdn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0x0F,	//	n=>0x0F
					[0x02] = 0x56,	//	add c,[n] => 0xFF
					[0x03] = 0x20,
					[0x20] = 0xF0,	//	[c]=>0xF0
				}
			};
			addcdn.CopyProgramToExpected();
			Add(addcdn, new DebugCpuState() { C = 0xFF, IP = 0x03, NoCarry = true });
		}

		/// <summary>
		/// Tests to represent adding values into the memory at [n]
		/// </summary>
		private void AddDirectNTests()
		{
			var adddna = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0xAA,	//	n=>0xAA
					[0x02] = 0x58,	//	add [n],a => 0xDA in [0x20]
					[0x03] = 0x20,	//	n=>0x20
					[0x20] = 0x30,
				}
			};
			adddna.CopyProgramToExpected();
			adddna.Expected[0x20] = 0xDA;
			Add(adddna, new DebugCpuState() { A = 0xAA, IP = 0x04, NonZero = true, NoCarry = true, ParityEven = true });
			
			var adddnb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	add b,n
					[0x01] = 0xBB,
					[0x02] = 0x59,	//	add [n],b => 0xDB in [0x20]
					[0x03] = 0x20,
					[0x20] = 0x20,
				}
			};
			adddnb.CopyProgramToExpected();
			adddna.Expected[0x20] = 0xDB;
			Add(adddnb, new DebugCpuState() { B = 0xBB, IP = 0x04, NoCarry = true });

			var adddnc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	add c,n
					[0x01] = 0xCC,
					[0x02] = 0x5A,	//	add [n],c => 0xDC in [0x20]
					[0x03] = 0x20,
					[0x20] = 0x10,
				}
			};
			adddnc.CopyProgramToExpected();
			adddna.Expected[0x20] = 0xDC;
			Add(adddnc, new DebugCpuState() { C = 0xCC, IP = 0x04, NoCarry = true });
			
			var adddnn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x5B,	//	add [n],n
					[0x01] = 0x20,
					[0x02] = 0xD0,
					[0x20] = 0x0D,
				}
			};
			adddnn.CopyProgramToExpected();
			adddna.Expected[0x20] = 0xDD;
			Add(adddnn, new DebugCpuState() { IP = 0x03, NoCarry = true });
			
			var adddndb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	add b,n
					[0x01] = 0x20,	//  	
					[0x02] = 0x5C,	//	add [n],[b]
					[0x03] = 0x21,
					[0x20] = 0xDB,
					[0x21] = 0x40,
					[0x40] = 0xFF,	//	Force a value here, which is where add [n],[b] shouadd write 0xDB
				}
			};
			adddndb.CopyProgramToExpected();
			adddndb.Expected[0x40] = 0x1B;
			//	add [n],[b] should result in a value of 0x11B, which overflows, so Carry should be set
			Add(adddndb, new DebugCpuState() { B = 0x20, IP = 0x04, Carry = true });
			
			var adddndc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	add c,n
					[0x01] = 0x20,	//	
					[0x02] = 0x5D,	//	add [n],[c]
					[0x03] = 0x40,
					[0x20] = 0xDD,
					[0x40] = 0xFF,	//	Force a value here, which is where add [n],[c] shouadd write 0xDC
				}
			};
			adddndc.CopyProgramToExpected();
			adddndb.Expected[0x40] = 0xDC;
			//	add [n],[c] should result in a value of 0x1DC, which overflows, so Carry should be set
			Add(adddndc, new DebugCpuState() { C = 0x20, IP = 0x04, Carry = true });
			
			var adddndn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x5E,	//	add [n],[n]
					[0x01] = 0x20,	//
					[0x02] = 0x40,
					[0x20] = 0xF0,	//	<= LHS value
					[0x40] = 0x0F,	//	<= RHS value
									//	add [n],[n] should write 0xFF here
				}
			};
			adddndn.CopyProgramToExpected();
			adddndn.Expected[0x40] = 0xFF;
			Add(adddndn, new DebugCpuState() { IP = 0x03, NoCarry = true });
		}
	}

	#endregion
	
	#region Tests for verifying Sub command behaviour

	public class SubCommandTestDataProvider
		: TheoryData<MachineMemory, CpuState>
	{
		public SubCommandTestDataProvider()
		{
			SubAccumulatorTests();
			SubRegisterBTests();
			SubRegisterCTests();
			SubDirectNTests();
		}

		/// <summary>
		/// Tests to represent subtracting values from the accumulator register
		/// </summary>
		private void SubAccumulatorTests()
		{
			var subaa = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x60,	//	sub a,a
				}
			};
			subaa.CopyProgramToExpected();
			Add(subaa, new DebugCpuState() { A = 0, IP = 0x01, Zero = true, ParityEven = true });

			var subaa2 = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 1,		//	n=>1
					[0x02] = 0x60,	//	sub a,a
				}
			};
			subaa2.CopyProgramToExpected();
			Add(subaa2, new DebugCpuState() { A = 0, IP = 0x03, Zero = true, ParityEven = true });
			
			var subab = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0x0B,	//	11 / 0x0B NZ, NC, PO
					[0x02] = 0x61,	//	sub a,b
				}
			};
			subab.CopyProgramToExpected();
			//	Subtracting b from a results in an underflow, so Minus should be set
			Add(subab, new DebugCpuState() { A = 0xF5, B = 0x0B, IP = 0x03, NonZero = true, Minus = true, ParityEven = true });

			var subac = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	sub c,n
					[0x01] = 0x0C,	//	12 / 0x0C NZ, NC, PE
					[0x02] = 0x62,	//	sub a,c
				}
			};
			subac.CopyProgramToExpected();
			Add(subac, new DebugCpuState() { A = 0xF4, C = 0x0C, IP = 0x03, NonZero = true, Minus = true, ParityOdd = true });
			
			var subad = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x43,	//	sub a,n
					[0x01] = 0x80,
				}
			};
			subad.CopyProgramToExpected();
			Add(subad, new DebugCpuState() { A = 0x80, IP = 0x02, NonZero = true, Minus = true, ParityOdd = true });
			
			var subadb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0xBB,
					[0x02] = 0x2B,	//	ld b,n
					[0x03] = 0x20,
					[0x04] = 0x64,	//	sub a,[b]
					[0x20] = 0x0B,
				}
			};
			subadb.CopyProgramToExpected();
			Add(subadb, new DebugCpuState() { A = 0xB0, B = 0x20, IP = 0x05, NonZero = true, NoCarry = true, ParityOdd = true });
			
			var subadc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0xCC,
					[0x02] = 0x33,	//	sub c,n
					[0x03] = 0x20,
					[0x04] = 0x65,	//	sub a,[c]
					[0x20] = 0x0C,
				}
			};
			subadc.CopyProgramToExpected();
			Add(subadc, new DebugCpuState() { A = 0xC0, C = 0x20, IP = 0x05, NonZero = true, NoCarry = true, ParityEven = true });
			
			var subadn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0xDD,
					[0x02] = 0x66,	//	sub a,[n]
					[0x03] = 0x20,
					[0x20] = 0x0D,
				}
			};
			subadn.CopyProgramToExpected();
			Add(subadn, new DebugCpuState() { A = 0xD0, IP = 0x04, NonZero = true, NoCarry = true, ParityOdd = true });
		}
				
		/// <summary>
		/// Tests to represent stracting values from the B register
		/// </summary>
		private void SubRegisterBTests()
		{
			var subba = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0x0A,
					[0x02] = 0x68,	//	sub b,a
				}
			};
			subba.CopyProgramToExpected();
			Add(subba, new DebugCpuState() { A = 0x0A, B = 0xF6, IP = 0x03, Minus = true });
			
			var subbb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x69,	//	sub b,b
				}
			};
			subbb.CopyProgramToExpected();
			Add(subbb, new DebugCpuState() { IP = 0x01, NoCarry = true });

			var subbb2 = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b, n
					[0x01] = 0x0B,	//	n=> 0x0B
					[0x02] = 0x69,	//	sub b,b
				}
			};
			subbb2.CopyProgramToExpected();
			Add(subbb2, new DebugCpuState() { IP = 0x03, NoCarry = true });

			var subbc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b, n
					[0x01] = 0xCC,
					[0x02] = 0x33,	//	ld c,n
					[0x03] = 0x0C,
					[0x04] = 0x6A,	//	sub b,c
				}
			};
			subbc.CopyProgramToExpected();
			Add(subbc, new DebugCpuState() { B = 0xC0, C = 0x0C, IP = 0x05, NoCarry = true });
			
			var subbd = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x6B,	//	sub b,n
					[0x01] = 0xDD,	//	221 / 0xDD NZ, NC, PE
				}
			};
			subbd.CopyProgramToExpected();
			Add(subbd, new DebugCpuState() { B = 0x23, IP = 0x02, Minus = true });
			
			var subbdb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0x20,
					[0x02] = 0x6C,	//	sub b,[b]
					[0x20] = 0x0B,
				}
			};
			subbdb.CopyProgramToExpected();
			Add(subbdb, new DebugCpuState() { B = 0x15, IP = 0x03, NoCarry = true });
			
			var subbdc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0xBB,
					[0x02] = 0x33,	//	ld c,n
					[0x03] = 0x20,	//	
					[0x04] = 0x6D,	//	sub b,[c]
					[0x20] = 0xEE,
				}
			};
			subbdc.CopyProgramToExpected();
			Add(subbdc, new DebugCpuState() { B = 0xDC, C = 0x20, IP = 0x05, Minus = true });
			
			var subbdn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0xDD,
					[0x02] = 0x6E,	//	sub b,[n]
					[0x03] = 0x20,
					[0x20] = 0x22,
				}
			};
			subbdn.CopyProgramToExpected();
			Add(subbdn, new DebugCpuState() { B = 0xBB, IP = 0x04, NoCarry = true });
		}

		/// <summary>
		/// Tests to represent subing values into the C register
		/// </summary>
		private void SubRegisterCTests()
		{
			var subca = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0xAA,
					[0x02] = 0x70,	//	sub c,a
				}
			};
			subca.CopyProgramToExpected();
			Add(subca, new DebugCpuState() { A = 0xAA, C = 0x56, IP = 0x03, Minus = true, NoCarry = true, ParityEven = true });
			
			var subcb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0xBB,
					[0x02] = 0x71,	//	sub c,b
				}
			};
			subcb.CopyProgramToExpected();
			Add(subcb, new DebugCpuState() { B = 0xBB, C = 0x45, IP = 0x03, Minus = true });

			var subcc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x72,	//	sub c,c
				}
			};
			subcc.CopyProgramToExpected();
			Add(subcc, new DebugCpuState() { IP = 0x01, NoCarry = true });
			
			var subcc2 = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0x0C,	//	n=>0x0C
					[0x02] = 0x72,	//	sub c,c
				}
			};
			subcc2.CopyProgramToExpected();
			Add(subcc2, new DebugCpuState() { IP = 0x03, NoCarry = true });
			
			var subcn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0xCC,
					[0x02] = 0x73,	//	sub c,n
					[0x03] = 0x0C,
				}
			};
			subcn.CopyProgramToExpected();
			Add(subcn, new DebugCpuState() { C = 0xC0, IP = 0x04, NoCarry = true });
			
			var subcdb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0x20,	//	
					[0x02] = 0x33,	//	ld c,n
					[0x03] = 0xD0,	//	208 / 0xD0 NZ, NC, PO
					[0x04] = 0x74,	//	sub c,[b] => 0xC5
					[0x20] = 0x0B,
				}
			};
			subcdb.CopyProgramToExpected();
			Add(subcdb, new DebugCpuState() { B = 0x20, C = 0xC5, IP = 0x05, NoCarry = true });
			
			var subcdc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0x20,
					[0x02] = 0x75,	//	sub c,[c] => 0x74
					[0x20] = 0xAC,
				}
			};
			subcdc.CopyProgramToExpected();
			Add(subcdc, new DebugCpuState() { C = 0x74, IP = 0x03, Minus = true });
			
			var subcdn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0xFF,	//	n=>0xFF
					[0x02] = 0x76,	//	sub c,[n] => 0xF0
					[0x03] = 0x20,
					[0x20] = 0x0F,
				}
			};
			subcdn.CopyProgramToExpected();
			Add(subcdn, new DebugCpuState() { C = 0xF0, IP = 0x04, NoCarry = true });
		}

		/// <summary>
		/// Tests to represent subtracting values from the memory at [n]
		/// </summary>
		private void SubDirectNTests()
		{
			var subdna = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23,	//	ld a,n
					[0x01] = 0xAA,	//	n=>0xAA
					[0x02] = 0x78,	//	sub [n],a => 0x33 in [0x20]
					[0x03] = 0x20,	//	n=>0x20
					[0x20] = 0xDD,
				}
			};
			subdna.CopyProgramToExpected();
			subdna.Expected[0x20] = 0x33;
			Add(subdna, new DebugCpuState() { A = 0xAA, IP = 0x04, NonZero = true, NoCarry = true, ParityEven = true });
			
			var subdnb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0xBB,
					[0x02] = 0x79,	//	sub [n],b => 0x22 in [0x20]
					[0x03] = 0x20,
					[0x20] = 0xDD,
				}
			};
			subdnb.CopyProgramToExpected();
			subdna.Expected[0x20] = 0x22;
			Add(subdnb, new DebugCpuState() { B = 0xBB, IP = 0x04, NoCarry = true });

			var subdnc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0xCC,
					[0x02] = 0x7A,	//	sub [n],c => 0x11 in [0x20]
					[0x03] = 0x20,
					[0x20] = 0xDD,
				}
			};
			subdnc.CopyProgramToExpected();
			subdna.Expected[0x20] = 0x11;
			Add(subdnc, new DebugCpuState() { C = 0xCC, IP = 0x04, NoCarry = true });
			
			var subdnn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x5B,	//	sub [n],n
					[0x01] = 0x20,
					[0x02] = 0xD0,
					[0x20] = 0xDD,
				}
			};
			subdnn.CopyProgramToExpected();
			subdna.Expected[0x20] = 0x0D;
			Add(subdnn, new DebugCpuState() { IP = 0x03, NoCarry = true });
			
			var subdndb = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B,	//	ld b,n
					[0x01] = 0x20,
					[0x02] = 0x7C,	//	sub [n],[b]
					[0x03] = 0x21,
					[0x20] = 0xDD,
					[0x21] = 0x40,
					[0x40] = 0xFF,	//	Result stored here should be 0xDE (0xDD - 0xFF)
				}
			};
			subdndb.CopyProgramToExpected();
			subdndb.Expected[0x40] = 0xDE;
			//	sub [n],[b] should result in a value of 0xFFDE, which underflows, so Minus should be set
			Add(subdndb, new DebugCpuState() { B = 0x20, IP = 0x04, Minus = true });
			
			var subdndc = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33,	//	ld c,n
					[0x01] = 0x20,	//	
					[0x02] = 0x7D,	//	sub [n],[c]
					[0x03] = 0x40,
					[0x20] = 0xDD,
					[0x40] = 0xFF,	//	Result stored here should be (0xFF - 0xDD) => 0x22
				}
			};
			subdndc.CopyProgramToExpected();
			subdndb.Expected[0x40] = 0x22;
			Add(subdndc, new DebugCpuState() { C = 0x20, IP = 0x04, NoCarry = true });
			
			var subdndn = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x7E,	//	sub [n],[n]
					[0x01] = 0x20,	//
					[0x02] = 0x40,
					[0x20] = 0xEA,	//	<= LHS value
					[0x40] = 0x0D,	//	<= RHS value
									//	sub [n],[n] should write 0xDD here
				}
			};
			subdndn.CopyProgramToExpected();
			subdndn.Expected[0x40] = 0xDD;
			Add(subdndn, new DebugCpuState() { IP = 0x03, NoCarry = true });
		}
	}

	#endregion
	
	#region Tests for verifying behaviour of push command
	
	public class PushCommandTestDataProvider
		: TheoryData<MachineMemory, CpuState>
	{
		public PushCommandTestDataProvider()
		{
			var testPushA = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x23, //	ld a
					[0x01] = 0xAA, //	0xAA
					[0x02] = 0xB0, //	push a
				}
			};
			testPushA.CopyProgramToExpected();
			testPushA.Expected[0xF7] = 0xAA;

			Add(testPushA, new DebugCpuState
			{
				A = 0xAA,
				IP = 0x03,
				SP = 0xF6,
				NonZero = true,
				NoCarry = true,
				ParityOdd = true,
			});
			
			var testPushB = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B, //	ld b
					[0x01] = 0xBB, //	0xBB
					[0x02] = 0xB1, //	push b
				}
			};
			testPushB.CopyProgramToExpected();
			testPushB.Expected[0xF7] = 0xBB;
			
			Add(testPushB, new DebugCpuState
			{
				B = 0xBB,
				IP = 0x03,
				SP = 0xF6,
				NoCarry = true,
			});
			
			var testPushC = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x33, //	ld c
					[0x01] = 0xCC, //	0xCC
					[0x02] = 0xB2, //	push c
				}
			};
			testPushC.CopyProgramToExpected();
			testPushC.Expected[0xF7] = 0xCC;
			
			Add(testPushC, new DebugCpuState
			{
				C = 0xCC,
				IP = 0x03,
				SP = 0xF6,
				NoCarry = true,
			});
			
			var testPushN = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0xB3, //	push n
					[0x01] = 0xAA, //	0xAA
				}
			};
			testPushN.CopyProgramToExpected();
			testPushN.Expected[0xF7] = 0xAA;
			
			Add(testPushN, new DebugCpuState
			{
				IP = 0x02,
				SP = 0xF6,
			});
			
			var testPushDirectB = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B, //	ld b
					[0x01] = 0x20, //	0x20
					[0x02] = 0xB4, //	push [b]
					[0x20] = 0xBB, //	data for push
				}
			};
			testPushDirectB.CopyProgramToExpected();
			testPushDirectB.Expected[0xF7] = 0xBB;
			
			Add(testPushDirectB, new DebugCpuState
			{
				B = 0x20,
				IP = 0x03,
				SP = 0xF6,
				NoCarry = true,
			});
			
			var testPushDirectC = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0x2B, //	ld b
					[0x01] = 0x20, //	0x20
					[0x02] = 0xB5, //	push [c]
					[0x20] = 0xCC, //	data for push
				}
			};
			testPushDirectC.CopyProgramToExpected();
			testPushDirectC.Expected[0xF7] = 0xCC;
			
			Add(testPushDirectC, new DebugCpuState
			{
				C = 0x20,
				IP = 0x03,
				SP = 0xF6,
				NoCarry = true,
			});
			
			var testPushDirectN = new MachineMemory()
			{
				Program =
				{
					[0x00] = 0xB6, //	push [n]
					[0x01] = 0x20, //	0x20
					[0x20] = 0xDD, //	data for push
				}
			};
			testPushDirectN.CopyProgramToExpected();
			testPushDirectN.Expected[0xF7] = 0xDD;
			
			Add(testPushDirectN, new DebugCpuState
			{
				IP = 0x02,
				SP = 0xF6,
				// NoCarry = true,
			});
		}
	}

	#endregion
}