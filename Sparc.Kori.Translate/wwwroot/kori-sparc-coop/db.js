import Dexie from 'https://npmcdn.com/dexie/dist/dexie.min.js';

const db = new Dexie('KoriTranslate');

db.version(1).stores({ translations: 'id' });

export default db;