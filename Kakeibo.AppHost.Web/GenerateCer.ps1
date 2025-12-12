# Path to your PFX
$pfxPath = "E:\CSharp\Kakeibo\Kakeibo.AppHost\Kakeibo.AppHost.Web\certificate.pfx"
# Output path for CER
$cerPath = "E:\CSharp\Kakeibo\Kakeibo.AppHost\Kakeibo.AppHost.Web\certificate.cer"

# Load PFX (you'll need the password)
$pfx = New-Object System.Security.Cryptography.X509Certificates.X509Certificate2
$pfx.Import($pfxPath, "B1feAceb0l@d0", [System.Security.Cryptography.X509Certificates.X509KeyStorageFlags]::Exportable)

# Export as CER (public key only, no private key)
[System.IO.File]::WriteAllBytes($cerPath, $pfx.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert))
