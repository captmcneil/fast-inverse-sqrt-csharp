namespace FastInverseSqrtSharp
{
	using System;
	using System.Diagnostics;
	using System.Linq;

	class Program
	{
		static void Main()
		{
			const int length = 100_000_000;
			var rng = new Random();

			// Make a collection of random floats
			// We use an array instead of a list to avoid any ADT related overhead.
			// You could experiment here with different number ranges, but it does not seem to
			// make a meaningful difference.
			float[] randomFloats = Enumerable.Range(0, length)
			                                 .Select(_ => rng.Next(0, 10000) / 10f)
			                                 .ToArray();

			// Store results, so we have proof the compiler computed the values and didn't cheat
			// something away, like unused variables.
			// We use arrays instead of Stack<T> or List<T>, since Push/Add seems to be slower.
			var fisqrtResults = new float[length];
			var nativeResults = new double[length];

			Console.WriteLine("Press any key to start.");
			Console.ReadKey();
			Console.WriteLine("Go ...");

			////////////////////////////////////////////////////////////////////////////////////
			// FastInverseSqrt

			var stopwatch = new Stopwatch();
			stopwatch.Start();

			// Note: foreach is faster than the good ol' for-loop - even on an array.
			// Even though we need the index anyway to add the value to the result array, this
			// seems to be faster.
			int index = 0;
			foreach (var eachNumber in randomFloats)
			{
				fisqrtResults[index] = FastInverseSqrt(eachNumber);
				index += 1;
			}

			stopwatch.Stop();
			Console.WriteLine($"„FastInverseSqrt“: {stopwatch.ElapsedMilliseconds}ms.");
			Console.ReadKey();

			////////////////////////////////////////////////////////////////////////////////////
			// Na(t)ive approach

			stopwatch = new Stopwatch();
			stopwatch.Start();

			index = 0;
			foreach (var eachNumber in randomFloats)
			{
				nativeResults[index] = InverseSqrt(eachNumber);
				index += 1;
			}
			
			stopwatch.Stop();
			Console.WriteLine($"Native „1 / Math.Sqrt“: {stopwatch.ElapsedMilliseconds}ms.");
			Console.ReadKey();

			////////////////////////////////////////////////////////////////////////////////////

			// print out some values, so the compiler really can't optimize anything away,
			// just because we don't use anything
			Console.WriteLine($"Random proof for „FastInverseSqrt“: {fisqrtResults[rng.Next(length)]}");
			Console.WriteLine($"Random proof for „1 / Math.Sqrt“:   {nativeResults[rng.Next(length)]}");
		}

		static float FastInverseSqrt(float number)
		{
			unsafe
			{
				long i;
				float x2, y;
				const float threehalfs = 1.5F;

				x2 = number * 0.5F;
				y = number;
				i = *(long*)&y;                        // evil floating point bit level hacking
				i = 0x5f3759df - (i >> 1);             // what the fuck? 
				y = *(float*)&i;
				y = y * (threehalfs - (x2 * y * y));   // 1st iteration
				return y;
			}
		}

		static double InverseSqrt(double number)
			=> 1d / Math.Sqrt(number);
	}
}
