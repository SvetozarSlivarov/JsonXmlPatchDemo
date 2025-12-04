using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using System.Xml.XPath;

#region JSON Patch

public class JsonPatchOperation
{
    public string Op { get; set; } = "";
    public string Path { get; set; } = "";
    public JsonNode? Value { get; set; }
}

public static class JsonPatchApplier
{
    public static JsonNode Apply(JsonNode target, IEnumerable<JsonPatchOperation> ops)
    {
        foreach (var op in ops)
        {
            ApplyOperation(target, op);
        }

        return target;
    }

    private static void ApplyOperation(JsonNode target, JsonPatchOperation op)
    {
        if (string.IsNullOrWhiteSpace(op.Path))
            throw new InvalidOperationException("Path cannot be empty.");

        var tokens = op.Path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);

        JsonNode parent = target;
        for (int i = 0; i < tokens.Length - 1; i++)
        {
            parent = GetChild(parent, tokens[i])
                     ?? throw new Exception("Invalid path: " + op.Path);
        }

        string last = tokens[^1];

        switch (op.Op.ToLower())
        {
            case "add":
                Add(parent, last, op.Value);
                break;
            case "remove":
                Remove(parent, last);
                break;
            case "replace":
                Replace(parent, last, op.Value);
                break;
            default:
                throw new Exception("Unsupported operation: " + op.Op);
        }
    }

    private static JsonNode? GetChild(JsonNode node, string token)
    {
        if (node is JsonObject obj)
            return obj[token];

        if (node is JsonArray arr && int.TryParse(token, out int i))
        {
            if (i >= 0 && i < arr.Count)
                return arr[i];
        }

        return null;
    }

    private static void Add(JsonNode parent, string token, JsonNode? value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        if (parent is JsonObject obj)
        {
            obj[token] = value;
        }
        else if (parent is JsonArray arr)
        {
            if (token == "-")
            {
                arr.Add(value);
            }
            else if (int.TryParse(token, out int index))
            {
                if (index < 0 || index > arr.Count)
                    throw new InvalidOperationException("Invalid index for add: " + index);

                arr.Insert(index, value);
            }
            else
            {
                throw new InvalidOperationException("Invalid array token: " + token);
            }
        }
        else
        {
            throw new InvalidOperationException("Add operation can only be applied to objects or arrays.");
        }
    }

    private static void Replace(JsonNode parent, string token, JsonNode? value)
    {
        if (value is null) throw new ArgumentNullException(nameof(value));

        if (parent is JsonObject obj)
        {
            if (!obj.ContainsKey(token))
                throw new InvalidOperationException($"Property '{token}' does not exist for replace.");

            obj[token] = value;
        }
        else if (parent is JsonArray arr && int.TryParse(token, out int index))
        {
            if (index < 0 || index >= arr.Count)
                throw new InvalidOperationException($"Invalid index for replace: {index}");

            arr[index] = value;
        }
        else
        {
            throw new InvalidOperationException("Replace operation can only be applied to objects or arrays.");
        }
    }

    private static void Remove(JsonNode parent, string token)
    {
        if (parent is JsonObject obj)
        {
            obj.Remove(token);
        }
        else if (parent is JsonArray arr && int.TryParse(token, out int index))
        {
            if (index < 0 || index >= arr.Count)
                throw new InvalidOperationException($"Invalid index for remove: {index}");

            arr.RemoveAt(index);
        }
        else
        {
            throw new InvalidOperationException("Remove operation can only be applied to objects or arrays.");
        }
    }
}

#endregion

#region XML Patch (custom)

public static class XmlPatcher
{
    public static void Apply(string xmlPath, string patchPath, string outputPath)
    {
        XDocument doc = XDocument.Load(xmlPath);
        XDocument patch = XDocument.Load(patchPath);

        foreach (var op in patch.Root!.Elements())
        {
            string type = op.Name.LocalName;
            string path = op.Attribute("path")?.Value 
                          ?? throw new InvalidOperationException("Missing 'path' attribute.");
            XElement? target = doc.XPathSelectElement(path);

            switch (type)
            {
                case "Replace":
                    ApplyReplace(target, op);
                    break;
                case "Remove":
                    ApplyRemove(target);
                    break;
                case "Add":
                    ApplyAdd(target, op);
                    break;
                default:
                    throw new InvalidOperationException("Unknown XML patch element: " + type);
            }
        }

        doc.Save(outputPath);
    }

    private static void ApplyReplace(XElement? target, XElement op)
    {
        if (target == null) return;

        string value = op.Attribute("value")?.Value 
                       ?? throw new InvalidOperationException("Replace requires 'value' attribute.");

        target.Value = value;
    }

    private static void ApplyRemove(XElement? target)
    {
        target?.Remove();
    }

    private static void ApplyAdd(XElement? target, XElement op)
    {
        if (target == null) return;

        foreach (var child in op.Elements())
        {
            target.Add(new XElement(child));
        }
    }
}

#endregion

class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        Console.WriteLine("== JSON Patch and XML Patch Demonstration ==");
        Console.WriteLine("1) Apply JSON Patch");
        Console.WriteLine("2) Apply XML Patch");
        Console.WriteLine("3) Apply both");
        Console.Write("Choose an option: ");

        var choice = Console.ReadLine();

        try
        {
            switch (choice)
            {
                case "1":
                    RunJsonPatch();
                    break;
                case "2":
                    RunXmlPatch();
                    break;
                case "3":
                    RunJsonPatch();
                    RunXmlPatch();
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred:");
            Console.WriteLine(ex.Message);
        }

        Console.WriteLine("Done. Press Enter to exit.");
        Console.ReadLine();
    }

    private static void RunJsonPatch()
    {
        const string jsonInput = "user_full.json";
        const string jsonPatch = "patch_full.json";
        const string jsonOutput = "user_full_patched.json";

        Console.WriteLine();
        Console.WriteLine("== JSON Patch ==");

        string json = File.ReadAllText(jsonInput);
        string patch = File.ReadAllText(jsonPatch);

        var target = JsonNode.Parse(json)
                     ?? throw new Exception("Failed to parse JSON file.");

        var ops = JsonSerializer.Deserialize<List<JsonPatchOperation>>(patch,
                     new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                  ?? new List<JsonPatchOperation>();

        var result = JsonPatchApplier.Apply(target, ops);

        File.WriteAllText(jsonOutput,
            result.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

        Console.WriteLine($"JSON Patch successfully applied. Output: {jsonOutput}");
    }

    private static void RunXmlPatch()
    {
        const string xmlInput = "user_full.xml";
        const string xmlPatch = "patch_full.xml";
        const string xmlOutput = "user_full_patched.xml";

        Console.WriteLine();
        Console.WriteLine("== XML Patch (custom) ==");

        XmlPatcher.Apply(xmlInput, xmlPatch, xmlOutput);

        Console.WriteLine($"XML patch successfully applied. Output: {xmlOutput}");
    }
}
