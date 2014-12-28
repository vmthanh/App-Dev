using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;

namespace PersonalNoteProject.Src.Classes
{
   public class CategoryRepository
    {
       public List<CategoryList> Load()
       {
           List<CategoryList> storedData;
           if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue("CategoryList", out storedData))
           {
               storedData = new List<CategoryList>();
           }
           return storedData;
       }

       public  void Save(List<CategoryList> categoryList)
       {
           IsolatedStorageSettings.ApplicationSettings["CategoryList"] = categoryList;
           IsolatedStorageSettings.ApplicationSettings.Save();
       }

       public  void Clear()
       {
           IsolatedStorageSettings.ApplicationSettings.Remove("CategoryList");
           IsolatedStorageSettings.ApplicationSettings.Save();
       }
    }
}
