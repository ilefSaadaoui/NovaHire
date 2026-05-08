
$baseUrl = "http://localhost:5000/api"
$results = @()

function Test-Endpoint {
    param($method, $path, $body, $token, $name, $contentType)
    
    $headers = @{}
    if ($token) { $headers.Add("Authorization", "Bearer $token") }
    
    $url = "$baseUrl$path"
    $start = Get-Date
    try {
        if ($method -eq "GET" -or $method -eq "DELETE") {
            $response = Invoke-RestMethod -Uri $url -Method $method -Headers $headers -ErrorAction Stop
        } else {
            if ($contentType -eq "multipart/form-data") {
                # Simplified multipart for test (just the fields)
                $boundary = [System.Guid]::NewGuid().ToString()
                $headers.Add("Content-Type", "multipart/form-data; boundary=$boundary")
                
                $LF = "`r`n"
                $innerBody = ""
                foreach ($key in $body.Keys) {
                    $innerBody += "--$boundary$LF"
                    $innerBody += "Content-Disposition: form-data; name=""$key""$LF$LF"
                    $innerBody += "$($body[$key])$LF"
                }
                $innerBody += "--$boundary--$LF"
                $response = Invoke-RestMethod -Uri $url -Method $method -Headers $headers -Body $innerBody -ErrorAction Stop
            } else {
                $headers.Add("Content-Type", "application/json")
                $response = Invoke-RestMethod -Uri $url -Method $method -Headers $headers -Body $body -ErrorAction Stop
            }
        }
        $duration = (Get-Date) - $start
        return @{ Name = $name; Method = $method; Path = $path; Status = "OK"; Duration = $duration.TotalMilliseconds; Message = "Success" }
    } catch {
        $duration = (Get-Date) - $start
        $msg = $_.Exception.Message
        if ($_.Exception.Response) {
            $stream = $_.Exception.Response.GetResponseStream()
            $reader = New-Object System.IO.StreamReader($stream)
            $msg = $reader.ReadToEnd()
        }
        return @{ Name = $name; Method = $method; Path = $path; Status = "FAIL"; Duration = $duration.TotalMilliseconds; Message = $msg }
    }
}

# 1. Login
write-host "Automated API Test Suite" -ForegroundColor Cyan
$saToken = Get-Content access_token.txt -Raw
$caToken = (Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body '{"email":"cadmin@neoledge.com", "password":"Password123!"}' -ContentType "application/json").accessToken
$reToken = (Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body '{"email":"recruiter@neoledge.com", "password":"Password123!"}' -ContentType "application/json").accessToken

# Get an offer token
$offers = Invoke-RestMethod -Uri "$baseUrl/JobOffer" -Method Get -Headers @{Authorization="Bearer $reToken"}
$offerId = $offers[0].id
$offerToken = $offers[0].shareToken

# --- PUBLIC ---
$results += Test-Endpoint "GET" "/Public/offers/$offerToken" $null $null "Public - View Offer"
$appBody = @{
    shareToken = $offerToken
    firstName = "John"
    lastName = "Doe"
    email = "john.doe@test.com"
    phone = "0102030405"
    coverLetter = "Motivated candidate"
}
$results += Test-Endpoint "POST" "/Public/applications" $appBody $null "Public - Submit Application" "multipart/form-data"

# --- COMPANY ADMIN ---
$results += Test-Endpoint "GET" "/CompanyAdmin/stats" $null $caToken "CompanyAdmin - Stats"

# Summary Table
$results | Format-Table -AutoSize
