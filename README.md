# JSON & XML Patch Demo (C# / .NET)

This project demonstrates the differences between JSON Patch (RFC 6902) and a custom XML Patch mechanism implemented in C#.  
Its purpose is to show that JSON Patch is a standardized, widely used method for partially updating JSON documents, while XML does not have an equivalent standard.

## 📄 Full Documentation (PDF)

The complete project documentation, including technical explanation, architecture, examples, and conclusions, is available here:

👉 **[Download Documentation (PDF)](docs/Documentation.pdf)**

---

## Project Goals
- Demonstrate a real JSON Patch implementation in .NET  
- Apply partial updates to a complex JSON structure  
- Show why XML requires custom patch logic  
- Compare JSON Pointer vs XPath  
- Highlight conceptual and practical differences between JSON and XML patching  

## Project Structure
JsonXmlPatchDemo/
│
├── Program.cs
├── JsonXmlPatchDemo.csproj
│
├── user_full.json
├── patch_full.json
│
├── user_full.xml
├── patch_full.xml
│
│
└── docs/
└── Documentation.pdf

## Running the Project
Run the following command:

dotnet run

You will see:

== JSON Patch and XML Patch Demonstration ==
1) Apply JSON Patch
2) Apply XML Patch
3) Apply both
Choose an option:

Selecting:
1 -> applies JSON Patch  
2 -> applies XML Patch  
3 -> applies both  

Output files will be generated in the build directory.

## JSON Patch Example (patch_full.json)

[
  { "op": "replace", "path": "/email", "value": "svetozar.petrov@example.com" },
  { "op": "add", "path": "/roles/-", "value": "admin" },
  { "op": "remove", "path": "/settings/privacy/ads" },
  { "op": "add", "path": "/settings/privacy/dataSharing", "value": false },
  { "op": "replace", "path": "/profile/contacts/phone", "value": "+359888555777" }
]

Supported operations:
- add
- remove
- replace

JSON Pointer example: /profile/contacts/phone

## XML Patch Example (patch_full.xml)

<Patch>
  <Replace path="/User/Email" value="svetozar.petrov@example.com" />

  <Add path="/User/Roles">
    <Role>admin</Role>
  </Add>

  <Remove path="/User/Settings/Privacy/Ads" />

  <Add path="/User/Settings/Privacy">
    <DataSharing>false</DataSharing>
  </Add>
</Patch>

XML uses:
- XPath for addressing nodes
- Custom Add / Remove / Replace elements

This demonstrates that XML has no official patch standard.

## Comparison: JSON Patch vs XML Patch

Feature | JSON Patch | XML Patch
--------|-------------|------------
Standard | RFC 6902 | No standard
Addressing | JSON Pointer | XPath
Add operation | Built-in | Manual
Remove | Built-in | Manual
Replace | Built-in | Manual
Complexity | Simple | More complex
Real-world usage | Common (REST APIs) | Rare

## Why XML Has No Patch Equivalent
- XML has no native array model  
- JSON Patch is tightly connected to JSON’s structure  
- XML tools like XSLT/XQuery Update are too complex for simple patching  
- No lightweight XML patch standard exists  

## Requirements
- .NET SDK 8 or newer
- (Optional) VS Code with C# Dev Kit

## License
Free for educational use.

## Author
Svetozar Slivarov
