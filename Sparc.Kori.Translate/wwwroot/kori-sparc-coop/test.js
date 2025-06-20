/**
 * translate.js
 * 
 * Dependency: PouchDB (https://pouchdb.com/)
 * Include this before this script:
 * <script src="https://cdn.jsdelivr.net/npm/pouchdb@9.0.0/dist/pouchdb.min.js"></script>
 */

if (typeof PouchDB === 'undefined') {
    throw new Error('translate.js requires PouchDB. Include PouchDB before this script.');
}

const dbName = `translations-${window.location.host.toLowerCase().replace(/[^a-zA-Z0-9]/g, '_')}`;
const db = new PouchDB(dbName);
const remote = 'https://localhost:7185/data/' + dbName;

// recursively collect all text nodes
function getTextNodes(node, nodes = []) {
    if (node.nodeType === Node.TEXT_NODE && node.nodeValue.trim()) {
        nodes.push(node);
    } else if (node.nodeType === Node.ELEMENT_NODE && node.tagName !== 'SCRIPT' && node.tagName !== 'STYLE') {
        for (let child of node.childNodes) {
            getTextNodes(child, nodes);
        }
    }
    return nodes;
}

// save text nodes to PouchDB
async function saveTextNodesAsync(nodes) {
    for (let n of nodes) {
        await saveKoriTextContentAsync(n);
    }
}

async function saveKoriTextContentAsync(node) {
    console.debug('navigator.language', navigator.language);

    const doc = {
        "$type": 'TextContent',
        _id: text.replace(/[\/\\#]/g, '_'),
        Domain: window.location.host,
        LanguageId: node.parentElement ? node.parentElement.lang || navigator.language : navigator.language,
        Text: node.nodeValue.trim()
    };

    try {
        await db.put(doc);
    } catch (e) {
        if (e.status !== 409) { // 409 = conflict already exists
            console.error(e);
        }
    }
}

// initial crawl
async function crawlAndSaveAsync() {
    console.debug('crawlAndSaveAsync');
    const nodes = getTextNodes(document.body);
    await saveTextNodesAsync(nodes);
}

// mutation observer to watch for new text nodes
const observer = new MutationObserver(mutations => {
    for (let mutation of mutations) {
        for (let node of mutation.addedNodes) {
            const newTextNodes = getTextNodes(node);
            if (newTextNodes.length) {
                saveTextNodesAsync(newTextNodes).catch(console.error);
            }
        }
    }
});

// start observing after initial crawl
function startObserving() {
    console.debug('startObserving');
    observer.observe(document.body, { childList: true, subtree: true });
}

// detect page changes
function setupPageChangeListener() {
    console.debug('setupPageChangeListener');

    window.addEventListener('popstate', () => {
        crawlAndSaveAsync().catch(console.error);
    });
    window.addEventListener('pushstate', () => {
        crawlAndSaveAsync().catch(console.error)
    });
}

// initialize everything
async function initTranslationCrawlerAsync() {
    await crawlAndSaveAsync();
    startObserving();
    setupPageChangeListener();

    // wait 5 seconds and then sync
    setTimeout(syncKoriTextContent, 15000);
}

function syncKoriTextContent() {
    console.debug('syncKoriTextContent');
    db.replicate.to(remote)
        .on('complete', info => {
            console.log('Sync complete:', info);
        })
        .on('error', err => {
            console.error('Sync error:', err);
        });
}

// run on DOMContentLoaded
document.addEventListener('DOMContentLoaded', initTranslationCrawlerAsync);
