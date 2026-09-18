import fs from 'fs';

async function update() {
    const resLogin = await fetch('http://localhost:5073/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: 'admin@company.com', password: 'Admin123!' })
    });
    const { accessToken } = await resLogin.json();

    const resGet = await fetch('http://localhost:5073/api/ai/configuration', {
        headers: { 'Authorization': `Bearer ${accessToken}` }
    });
    const config = await resGet.json();

    config.llmRerankingEnabled = true;
    config.llmRerankingTopK = 3;

    const resPut = await fetch('http://localhost:5073/api/ai/configuration', {
        method: 'PUT',
        headers: { 
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${accessToken}` 
        },
        body: JSON.stringify(config)
    });
    console.log(await resPut.text());
}
update();
