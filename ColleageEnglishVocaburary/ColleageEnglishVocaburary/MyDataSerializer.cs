using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace ColleageEnglishVocaburary
{
    public class MyDataSerializer<TheDataType>     
     {
         /// <summary>
         /// To Use:
         /// List<MyDataObjects> myObjects = ...      
         /// await MyDataSerializer<List<MyDataObjects>>.SaveObjectsAsync(myObjects, "MySerializedObjects.xml"); 
         /// </summary>
         /// <param name="sourceData"></param>
         /// <param name="targetFileName"></param>
         /// <returns></returns>
         public static async Task SaveObjectsAsync(TheDataType sourceData, String targetFileName)
         {
             StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                 targetFileName, CreationCollisionOption.ReplaceExisting);
             var outStream = await file.OpenStreamForWriteAsync();
             DataContractSerializer serializer = new DataContractSerializer(typeof (TheDataType));
             serializer.WriteObject(outStream, sourceData);
             await outStream.FlushAsync();
             outStream.Close();
         }

         /// <summary>
         /// To Use:
         /// List<MyDataObjects> myObjects = await MyDataSerializer<List<MyDataObjects>>.RestoreObjectsAsync("MySerializedObjects.xml");
         /// </summary>
         /// <param name="fileName"></param>
         /// <returns></returns>
         public static async Task<TheDataType> RestoreObjectsAsync(string fileName)
         {
             StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
             var inStream = await file.OpenStreamForReadAsync();
             // Deserialize the objects.           
             DataContractSerializer serializer = new DataContractSerializer(typeof (TheDataType));
             TheDataType data = (TheDataType) serializer.ReadObject(inStream);
             inStream.Close();
             return data;
         }

     } 

}
