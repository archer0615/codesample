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
             return System.IO.Path.ChangeExtension("Enums\\" +  File.Name, settings.OutputExtension);
        };
    }

    // Custom extension methods can be used in the template by adding a $ prefix e.g. $LoudName
    string LoudName(Property property)
    {
        return property.Name.ToUpperInvariant();
    }
    //module ROG.Scripts {}

    // $Classes/Enums/Interfaces(filter)[template][separator]
    // filter (optional): Matches the name or full name of the current item. * = match any, wrap in [] to match attributes or prefix with : to match interfaces or base classes.
    // template: The template to repeat for each matched item
    // separator (optional): A separator template that is placed between all templates e.g. $Properties[public $name: $Type][, ]
    //e => e.Namespace == "ROG.DataDefine" && e.Name == "Enums" && e.NestedEnums.Count() > 0
    // More info: http://frhagn.github.io/Typewriter/
}
$Classes(e => e.Namespace == "ROG.DataDefine.Enums" && e.Name == "EnumClass" && e.NestedEnums.Count() > 0)[
namespace $Name { $NestedEnums[
    /* $DocComment */
    export enum $Name { $Values[
        /* $DocComment */
       $Name = $Value][,]
    }]
}]

