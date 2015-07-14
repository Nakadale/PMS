/*
 * Created by SharpDevelop.
 * User: ghelo
 * Date: 7/23/2013
 * Time: 2:17 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 


/*
 * Date: 12/10/2013
 * 1. Edited but in trimming .m00 from filename
 * 2. Filter tree nodes so that online files ending in *.zip is listed
 * 3. Run conversion in thread
 * */


 
using System;
using System.Windows.Forms;

namespace MDB2RNX
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
	}
}
