# Parameters
$certName = "CN=100.64.1.29"
$certPath = "E:\CSharp\Kakeibo\Kakeibo.AppHost\Kakeibo.AppHost.Web\certificate.pfx"
$certPassword = "B1feAceb0l@d0"

# Create self-signed certificate
$cert = New-SelfSignedCertificate -DnsName "100.64.1.29" -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(5)

# Export to PFX
Export-PfxCertificate -Cert $cert -FilePath $certPath -Password (ConvertTo-SecureString -String $certPassword -Force -AsPlainText)
