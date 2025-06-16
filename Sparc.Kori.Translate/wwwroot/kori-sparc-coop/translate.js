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

const db = new PouchDB('translations');

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
async function saveTextNodes(nodes) {
    for (let n of nodes) {
        const path = getNodePath(n);
        const text = n.nodeValue.trim();
        const hash = hashString(text);
        const id = `textnode_${path}_${hash}`;
        const doc = {
            _id: id,
            text: text,
            path: path,
            hash: hash
        };
        try {
            await db.put(doc);
        } catch (e) {
            if (e.status !== 409) { // 409 = conflict already exists
                console.error(e);
            }
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
function crawlAndSave() {
    console.debug('crawlAndSave');
    const nodes = getTextNodes(document.body);
    saveTextNodes(nodes);
}

// mutation observer to watch for new text nodes
const observer = new MutationObserver(mutations => {
    for (let mutation of mutations) {
        for (let node of mutation.addedNodes) {
            const newTextNodes = getTextNodes(node);
            if (newTextNodes.length) {
                saveTextNodes(newTextNodes);
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
        crawlAndSave();
    });
    window.addEventListener('pushstate', () => {
        crawlAndSave();
    });
}

// initialize everything
function initTranslationCrawl() {
    crawlAndSave();
    startObserving();
    setupPageChangeListener();
}

// run on DOMContentLoaded
document.addEventListener('DOMContentLoaded', initTranslationCrawl);
