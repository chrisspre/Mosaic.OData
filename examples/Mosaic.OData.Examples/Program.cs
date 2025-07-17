
using Mosaic.OData.CSDL;


using var reader = CsdlXmlReader.Create(@"..\..\..\data\example89.xml");
foreach (var token in reader.ReadTokens())
{
    Console.WriteLine(token);
}
    