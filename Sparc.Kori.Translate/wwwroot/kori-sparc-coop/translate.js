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

const db = new PouchDB('kori-translations');
const remote = 'https://localhost:7185/data/kori-translations';
function hashString(str) {
    let hash = 2166136261;
    for (let i = 0; i < str.length; i++) {
        hash ^= str.charCodeAt(i);
        hash += (hash << 1) + (hash << 4) + (hash << 7) + (hash << 8) + (hash << 24);
    }
    // hash positive and as string
    return (hash >>> 0).toString(36);
}

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
    const userLang = navigator.language || navigator.userLanguage || "en";

    const path = getNodePath(node);
    const text = node.nodeValue.trim();
    const hash = hashString(text);
    const safePath = path.replace(/[\/\\#]/g, '_');
    const id = `node_${safePath}_${hash}`;
    const parentElement = node.parentElement;
    const html = parentElement ? parentElement.outerHTML : "";
    const tag = parentElement ? parentElement.tagName.toLowerCase() : "unknown";
    const contentType = "text";

    const doc = {
        _id: id,
        tag: tag,
        text: text,
        html: html,
        contentType: contentType,
        audio: null,
        //nodes: [parentElement],
        path: path,
        hash: hash,
        translations: [
            {
                text: text,
                language: userLang
            }
        ]
    };

    try {
        await db.put(doc);
    } catch (e) {
        if (e.status !== 409) { // 409 = conflict already exists
            console.error(e);
        }
    }
}

// get a unique path for a node (for reference)
function getNodePath(node) {
    let path = [];
    while (node && node.parentNode) {
        let index = Array.prototype.indexOf.call(node.parentNode.childNodes, node);
        path.unshift(index);
        node = node.parentNode;
    }
    return path.join('/');
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
