$GitHubName="TransferWindowPlanner"
$PluginName="TransferWindowPlanner"
$CurseID="224116"
$KerbalStuffModID = 268

$UploadDir = "$($PSScriptRoot)\..\..\_Uploads\$($PluginName)"
$KerbalStuffWrapper = "D:\Programming\KSP\_Scripts\KerbalStuffWrapper\KerbalStuffWrapper.exe"


function MergeDevToMaster() {
    write-host -ForegroundColor Yellow "`r`nMERGING DEVELOP TO MASTER"

	git checkout master
	git merge --no-ff develop -m "Merge $($Version) to master"
	git tag -a "v$($Version)" -m "Released version $($Version)"

	write-host -ForegroundColor Yellow "`r`nPUSHING MASTER AND TAGS TO GITHUB"
	git push
	git push --tags
	
	write-host -ForegroundColor Yellow "Back to Develop Branch"
    git checkout develop

	write-host -ForegroundColor Yellow "----------------------------"
	write-host -ForegroundColor Yellow "Finished Version $($Version)"
	write-host -ForegroundColor Yellow "----------------------------"
	
}

function CreateGitHubRelease() {
	write-host -ForegroundColor Yellow "`r`n Creating Release"

	$CreateBody = "{`"tag_name`":`"v$($Version)`",`"name`":`"v$($Version) Release`",`"body`":`"$($relDescr)`"}"
	
	$RestResult = Invoke-RestMethod -Method Post `
		-Uri "https://api.github.com/repos/TriggerAu/$($GitHubName)/releases" `
		-Headers @{"Accept"="application/vnd.github.v3+json";"Authorization"="token $($OAuthToken)"} `
		-Body $CreateBody
	if ($?)
	{
		write-host -ForegroundColor Yellow "Uploading File"
		$File = get-item "$($UploadDir)\v$($Version)\$($pluginname)_$($Version).zip"
		$RestResult = Invoke-RestMethod -Method Post `
			-Uri "https://uploads.github.com/repos/TriggerAu/$($GitHubName)/releases/$($RestResult.id)/assets?name=$($File.Name)" `
			-Headers @{"Accept"="application/vnd.github.v3+json";"Authorization"="token $($OAuthToken)";"Content-Type"="application/zip"} `
			-InFile $File.fullname
		
		"Result = $($RestResult.state)"
	}

	write-host -ForegroundColor Yellow "-----------------------------------"
	write-host -ForegroundColor Yellow "Finished GitHub Release $($Version)"
	write-host -ForegroundColor Yellow "-----------------------------------"
}

function CreateCurseRelease() {
    $CurseVersions = Invoke-RestMethod -method Get -uri "https://kerbal.curseforge.com/api/game/versions?token=$($CurseForgeToken)"
    $Choices= [System.Management.Automation.Host.ChoiceDescription[]] @("&Yes","&No")
    $ChoiceRtn = $host.ui.PromptForChoice("`r`nLatest Curse Version is $(($Curseversions | Sort-Object id -Descending)[0].Name)","Do you wish to upload v$($Version) to Curseforge for this KSP Version?",$Choices,0)
    if ($ChoiceRtn -eq 0 )
    {
        $metadata =  "{`"changelog`":`"$($relDescr)`", `"displayName`":`"v$($Version) Release`", `"gameVersions`": [$(($Curseversions | Sort-Object id -Descending)[0].id)], `"releaseType`": `"release`"}"
        
        $File = get-item "$($UploadDir)\v$($Version)\$($pluginname)_$($Version).zip"

        $filedata = [IO.File]::ReadAllBytes($File.fullname)

        $boundary = "--" + [System.Guid]::NewGuid().ToString()

        $body = @()
        $body += $boundary 
        $body += "content-disposition: form-data; name=`"metadata`"`n"
        $body += $metadata 
        $body += $boundary
        $body += "content-disposition: form-data; name=`"file`"`n"
        $body += $filedata
        $body += $boundary + "--`n"

        $RestResult = Invoke-RestMethod -Method Post `
			-Uri "https://kerbal.curseforge.com/api/projects/$($CurseID)/upload-file??token=$($CurseForgeToken)" `
            -Headers @{"X-Api-Token"=$($CurseForgeToken);} `
			-Body $body `
            -ContentType "multipart/form-data; boundary=$($boundary)"

		
		"Result = $($RestResult.state)"
        
    }
}

function CreateKerbalStuffRelease() {
    "Updating Mod at KerbalStuff"
    $File = get-item "$($UploadDir)\v$($Version)\$($pluginname)_$($Version).zip"
    & $KerbalStuffWrapper updatemod /m:$KerbalStuffModID /u:"$KerbalStuffLogin" /p:"$KerbalStuffPW" /k:"$KSPVersion" /v:"$Version" /f:"$($File.FullName)" /l:"$relKStuff" /n:true

}

#Get newest version
$Version =""
$VersionRead =  (Get-ChildItem $UploadDir -Filter "v*.*.*.*"|sort -Descending)[0].name.replace("v","")
if ($VersionRead -ne $null) {
	$Choices= [System.Management.Automation.Host.ChoiceDescription[]] @("&Yes","&No")
	$ChoiceRtn = $host.ui.PromptForChoice("Version v$($VersionRead) detected","Is this the version you wish to build?",$Choices,0)
	
	if($ChoiceRtn -eq 0){$Version = $VersionRead}
} 
if ($Version -eq "") {
	$Version = Read-Host -Prompt "Enter the Version Number to Publish" 
}


if ($Version -eq "")
{
    "No version string supplied... Quitting"
    return
}
else
{
    $Path = "$UploadDir\v$($Version)\$($PluginName)_$($Version)\GameData\TriggerTech\$($PluginName)\$($PluginName).dll"
    "DLL Path:`t$($Path)"
    if (Test-Path $Path)
    {
	    $dll = get-item $Path
	    $VersionString = $dll.VersionInfo.ProductVersion

        if ($Version -ne $VersionString) {
            "Versions dont match`r`nEntered:`t$Version`r`nFrom File:`t$VersionString"
            return
        } else {
            if ($GitHubToken -ne $null){
                $OAuthToken = $GitHubToken
            } else {
                $OAuthToken = Read-Host -Prompt "GitHub OAuth Token"
            }

            #if ($CurseForgeToken -eq $null -or $CurseForgeToken -eq "") {
            #    $CurseForgeToken = Read-Host -Prompt "CurseForge OAuth Token"
            #}

            if ($KerbalStuffPW -eq $null) {
                $KerbalStuffLogin = Read-Host -Prompt "KerbalStuff Login"
                $KerbalStuffPW = Read-Host -Prompt "KerbalStuff Password"
            }

        }
    } else {
        "Cant find the dll - have you built the dll first?"
        return
    }
}



"`r`nThis will Merge the devbranch with master and push the release of v$($Version) of the $($PluginName)"
"`tFrom:`t$UploadDir\v$($Version)"
"`tGitHub Oauth:`t$OAuthToken"
"`tCurse  Oauth:`t$CurseForgeToken"
"`tKerbalStuff:`t$KerbalStuffLogin : $KerbalStuffPW"

$Choices= [System.Management.Automation.Host.ChoiceDescription[]] @("&Yes","&No")
$ChoiceRtn = $host.ui.PromptForChoice("Do you wish to Continue?","Be sure develop is ready before hitting yes",$Choices,1)

if($ChoiceRtn -eq 0)
{
	#git add -A *
	#git commit -m "Version history $($Version)"
	
	#write-host -ForegroundColor Yellow "`r`nPUSHING DEVELOP TO GITHUB"
	#git push

    #MergeDevToMaster


    $readme = (Get-Content -Raw "$($PSScriptRoot)\..\PluginFiles\ReadMe-$($PluginName).txt")

    #If couldn't load it then bork out
    if (!$?) {
        "Couldn't load the readme file. Quitting..."
        return
    }
	$reldescr = [regex]::match($readme,"Version\s$($Version).+?(?=[\r\n]*Version\s\d+|$)","singleline,ignorecase").Value

	#Now get the KSPVersion from the first line
	$KSPVersion = [regex]::match($reldescr,"KSP\sVersion\:.+?(?=[\r\n]|$)","singleline,ignorecase").Value
	
	#Now drop the first line
	$reldescr = [regex]::replace($reldescr,"^.+?\r\n","","singleline,ignorecase")
	
	$reldescr = $reldescr.Trim("`r`n")
	$reldescr = $reldescr.Replace("- ","* ")
	$reldescr = $reldescr.Replace("`r`n","\r\n")
	$reldescr = $reldescr.Replace("`"","\`"")
	
    $ForumList = "[LIST]`r`n" + $reldescr + "`r`n[/LIST]"
    $ForumList = $ForumList.Replace("\r\n","`r`n").Replace("`r`n* ","`r`n[*]")

	$reldescr = "$($reldescr)\r\n\r\n``````$($KSPVersion)``````"

    $relKStuff = $reldescr.Replace("\r\n","`r`n")



    CreateGitHubRelease

    #CreateCurseRelease 

    CreateKerbalStuffRelease

}
else
{
    "Skipping..."
}
