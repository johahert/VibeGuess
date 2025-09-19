# Quick debug test
cd "c:\Users\johan\repos\vibeguess"
$env:ASPNETCORE_ENVIRONMENT="Development"
Start-Process "dotnet" -ArgumentList "run","--project","src/VibeGuess.Api" -PassThru -NoNewWindow
Start-Sleep 5

try {
    $headers = @{
        "Authorization" = "Bearer expired.jwt.token"
    }
    
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/playback/status" -Headers $headers -Method GET
    Write-Host "Status: $($response.StatusCode)"
    Write-Host "Content: $($response.Content)"
} catch {
    Write-Host "Error Status: $($_.Exception.Response.StatusCode)"
    Write-Host "Error Content: $($_.Exception.Response)"
    
    if ($_.Exception.Response) {
        $stream = $_.Exception.Response.GetResponseStream()
        $reader = New-Object System.IO.StreamReader($stream)
        $content = $reader.ReadToEnd()
        Write-Host "Error Body: $content"
    }
}