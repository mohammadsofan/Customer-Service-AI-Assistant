import fs from 'fs';

async function askQuestion(token, questionText) {
    const start = Date.now();
    const res = await fetch('http://localhost:5073/api/support/questions', {
        method: 'POST',
        headers: { 
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}` 
        },
        body: JSON.stringify({ problem: questionText })
    });
    const text = await res.text();
    let data;
    try {
        data = JSON.parse(text);
    } catch(e) {
        console.log(`Failed to parse JSON. Raw response: ${text}`);
        return;
    }
    console.log(`-----------------------------------------`);
    console.log(`Asking: "${questionText}"`);
    console.log(`Latency: ${Date.now() - start}ms`);
    console.log(`Status: ${data.status}`);
    console.log(`Matched Scenario ID: ${data.sourceScenario}`);
    console.log(`Answer:\n${data.answer}`);
}

async function runTests() {
    // using admin
    const adminLogin = await fetch('http://localhost:5073/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email: 'admin@company.com', password: 'Admin123!' })
    });
    const { accessToken } = await adminLogin.json();

    const testCases = [
        "المشترك بده يعرف قديش فاتورته عالجوال",
        "عندي مشكلة بالانترنت", 
        "انا غضبان جدا اريد الغاء اشتراكي فورا بدون اي نقاش" 
    ];

    for (const q of testCases) {
        await askQuestion(accessToken, q);
    }
}

runTests();
