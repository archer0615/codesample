${
    // Enable extension methods by adding using Typewriter.Extensions.*
    using Typewriter.Extensions.Types;

    // Uncomment the constructor to change template settings.
    Template(Settings settings)
    {

        //settings.IncludeProject("ROG");
        settings.OutputExtension = ".ts";
        settings.OutputFilenameFactory = File =>
        {    
            //DEBUG
            //System.IO.File.AppendAllText("D:\\ASUS\\rog\\ROG.WebAPI\\Scripts\\TS_TestFile.txt", File.FullName + Environment.NewLine);

            var dir = File.FullName.Split('\\');
            var paths = "";
            bool isViewModels = false;
            foreach(var item in dir)
            {
                    if (isViewModels)
                    {
                    paths = paths + item + "\\";
                    break;
                }
                if (item == "ViewModels")
                {
                    isViewModels = true;
                }
            }
            //System.IO.File.AppendAllText("D:\\ASUS\\rog\\ROG.WebAPI\\Scripts\\TS_TestFile.txt", paths + Environment.NewLine);
            return System.IO.Path.ChangeExtension(paths +  File.Name, settings.OutputExtension);
        };
    }

    // Custom extension methods can be used in the template by adding a $ prefix e.g. $LoudName
    string LoudName(Property property)
    {
        if(property.Type.FullName.Contains("ObjViewModel"))
        {
            return property.Type.FullName.Replace("ObjViewModel","") + "[]";
        }
        return property.Name.ToUpperInvariant();
    }
}

	$Classes(*ViewModel)[	
		export interface $Name 
        {  $Properties[
            $Name: $Type;]
		}       
    ]