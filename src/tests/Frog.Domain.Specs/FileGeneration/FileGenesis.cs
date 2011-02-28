using System;
using NUnit.Framework;
using System.IO;
using Frog.Specs.Support;

namespace Frog.Domain.Specs
{
	[TestFixture]
	public class FileGenesis
	{
		string _workPlace;
		
		[SetUp]
		public void Setup(){
            _workPlace = Path.Combine(GitTestSupport.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(_workPlace);			
		}
		
		[TearDown]
		public void Cleanup(){
            OSHelpers.ClearAttributes(_workPlace);
            Directory.Delete(_workPlace, true);
		}
		
		[Test]
		public void should_create_file_with_contents()
		{
			var genesis = new FileGen(_workPlace);
			genesis.File("Fle.txt", "aaaaa");
			
			Assert.That(System.IO.File.ReadAllText(Path.Combine(_workPlace, "Fle.txt")), Is.EqualTo("aaaaa"));
		}
	}
	
	public class FileGen
	{
		string root;
		
		public FileGen(string root){
			this.root = root;
		}
		
		public void File(string name, string content){
			System.IO.File.WriteAllText(Path.Combine(root, name), content);
		}
	}
}

