param($installPath, $toolsPath, $package, $project)

$project.Properties.Item("PostBuildEvent").Value = ""