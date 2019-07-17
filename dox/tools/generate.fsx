// --------------------------------------------------------------------------------------
// Builds the documentation from `.fsx` and `.md` files in the 'docs/content' directory
// (the generated documentation is stored in the 'docs/output' directory)
// --------------------------------------------------------------------------------------

// Web site location for the generated documentation
let website = "/FromObjectsToFunctions"

let githubLink = "https://github.com/danieljsummers/FromObjectsToFunctions"

// Specify more information about your project
let info =
  [ "project-name", "objects () |> functions"
    "project-author", "Daniel J. Summers"
    "project-summary", "An attempt to provide a concrete, working example to demonstrate to C# developers how F# can improve their workflow and performance"
    "project-github", githubLink
    "project-nuget", "http://nuget.org/packages/FromObjectsToFunctions" ]

// --------------------------------------------------------------------------------------
// For typical project, no changes are needed below
// --------------------------------------------------------------------------------------

#load "../../packages/FSharp.Formatting/FSharp.Formatting.fsx"
#I "../../packages/FAKE/tools/"
#r "FakeLib.dll"
open Fake
open System.IO
open FSharp.Formatting.Razor

// When called from 'build.fsx', use the public project URL as <root>
// otherwise, use the current 'output' directory.
#if RELEASE
let root = website
#else
let root = "file://" + (__SOURCE_DIRECTORY__ @@ "../output")
#endif

// Paths with template/source/output locations
let bin        = __SOURCE_DIRECTORY__ @@ "../../bin"
let content    = __SOURCE_DIRECTORY__ @@ "../content"
let output     = __SOURCE_DIRECTORY__ @@ "../output"
let files      = __SOURCE_DIRECTORY__ @@ "../files"
let templates  = __SOURCE_DIRECTORY__ @@ "templates"
let formatting = __SOURCE_DIRECTORY__ @@ "../../packages/FSharp.Formatting/"
let docTemplate = "docpage.cshtml"

// Where to look for *.csproj templates (in this order)
let layoutRootsAll = System.Collections.Generic.Dictionary<string, string list>()
layoutRootsAll.Add ("en",[ templates; formatting @@ "templates"; formatting @@ "templates/reference" ])
subDirectories (directoryInfo templates)
|> Seq.iter (fun d ->
    match d.Name.Length with
    | 2 | 3 ->
      layoutRootsAll.Add (
        d.Name,
        [templates @@ d.Name; formatting @@ "templates"; formatting @@ "templates/reference" ])
    | _ -> ())

// Copy static files and CSS + JS from F# Formatting
let copyFiles () =
  CopyRecursive files output true
  |> Log "Copying file: "
  ensureDirectory (output @@ "content")
  CopyRecursive (formatting @@ "styles") (output @@ "content") true 
  |> Log "Copying styles and scripts: "

// Build documentation from `fsx` and `md` files in `docs/content`
let buildDocumentation () =

  // First, process files which are placed in the content root directory.
  RazorLiterate.ProcessDirectory
    ( content, docTemplate, output,
      layoutRoots      = layoutRootsAll.["en"],
      replacements     = ("root", root)::info,
      generateAnchors  = true,
      processRecursive = false)

  // And then process files which are placed in the sub directories
  // (some sub directories might be for specific language).
  let subdirs = Directory.EnumerateDirectories(content, "*", SearchOption.TopDirectoryOnly)
  for dir in subdirs do
    let dirname = (new DirectoryInfo(dir)).Name
    let layoutRoots =
      // Check whether this directory name is for specific language
      let key = layoutRootsAll.Keys
                |> Seq.tryFind (fun i -> i = dirname)
      match key with
      | Some lang -> layoutRootsAll.[lang]
      | None -> layoutRootsAll.["en"] // "en" is the default language
    RazorLiterate.ProcessDirectory
      ( dir, docTemplate, output @@ dirname,
        layoutRoots     = layoutRoots,
        replacements    = ("root", root) :: info,
        generateAnchors = true )

// Generate
copyFiles()
buildDocumentation()
