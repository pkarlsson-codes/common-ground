<#macro emailLayout>
<html lang="${locale.language}" dir="${(ltr)?then('ltr','rtl')}">
<body style="margin:0;padding:0;background:#f4f6f9;">
  <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background:#f4f6f9;padding:24px 12px;">
    <tr><td align="center">
      <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;background:#ffffff;border:1px solid #dde3ea;border-radius:6px;">
        <tr><td style="background:#0b1018;padding:20px 28px;border-radius:6px 6px 0 0;border-bottom:3px solid #bf832a;">
          <span style="font-family:Georgia,'Times New Roman',serif;font-size:26px;color:#e8f2fc;">CommonGround</span>
        </td></tr>
        <tr><td style="padding:28px;font-family:Arial,Helvetica,sans-serif;font-size:15px;line-height:1.55;color:#1c2733;">
          <#nested>
        </td></tr>
        <tr><td style="padding:16px 28px;border-top:1px solid #dde3ea;font-family:Arial,Helvetica,sans-serif;font-size:12px;color:#6b7a8c;">
          Sent by CommonGround because someone used this email address there.
        </td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>
</#macro>
