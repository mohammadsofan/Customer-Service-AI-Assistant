const key = process.env.GEMINI_API_KEY;
const res2 = await fetch('https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-2:embedContent?key=' + key, {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    model: 'models/gemini-embedding-2',
    content: { parts: [{ text: 'Hello' }] }
  })
});
console.log('gemini-embedding-2:', res2.status, await res2.text());
