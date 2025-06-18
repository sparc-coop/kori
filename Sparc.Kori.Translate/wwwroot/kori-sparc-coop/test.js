if (typeof PouchDB === 'undefined') {
    throw new Error('test.js requires PouchDB. Include PouchDB before this script.');
}

const db = new PouchDB('kori-test');
const remote = 'https://localhost:7185/data/kori-test';

// generate random objects for testing
for (let i = 0; i < 100; i++) {
    const obj = {
        _id: `test_${i}`,
        name: `Test Object ${i}`,
        value: Math.random() * 1000
    };
    db.put(obj).then(() => {
        console.log(`Inserted: ${obj._id}`);
    }).catch(err => {
        if (err.status !== 409) { // 409 = conflict already exists
            console.error(`Error inserting ${obj._id}:`, err);
        }
    });
}

db.replicate.to(remote);