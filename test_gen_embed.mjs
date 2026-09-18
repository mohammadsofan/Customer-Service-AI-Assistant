const key = process.env.GEMINI_API_KEY;
const res = await fetch('https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-2:generateContent?key=' + key, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    contents: [{ parts: [{ text: 'Test connection' }] }]
  })
});
console.log('generateContent on gemini-embedding-2:', res.status, await res.text());
