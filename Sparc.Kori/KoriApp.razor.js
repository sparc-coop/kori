let element = {};
let observer = {};
let translations = {};
let dotNet = {};

let koriIgnoreFilter = function (node) {
    var approvedNodes = ['#text', 'IMG'];

    if (!approvedNodes.includes(node.nodeName) || node.parentNode.nodeName == 'SCRIPT')
        return NodeFilter.FILTER_SKIP;

    var closest = node.parentElement.closest('.kori-ignore');
    if (closest)
        return NodeFilter.FILTER_SKIP;

    return NodeFilter.FILTER_ACCEPT;
}

export async function init(elementSelector, translations, dotNetReference) {
    console.log("Initializing element", elementSelector);

    element = document.querySelector(elementSelector);
    translations = translations;
    dotNet = dotNetReference;

    var missingTranslations = await registerNodesUnder(element);

    observer = new MutationObserver(observeCallback);
    observer.observe(element, { childList: true, characterData: true, subtree: true });

    return missingTranslations;
}

export function getPageTitle() {
    return document.title;
}

async function registerNodesUnder(el) {
    var missingTranslations = [];
    var missingTranslation;
    var node, walk = document.createTreeWalker(el, NodeFilter.SHOW_TEXT | NodeFilter.SHOW_ELEMENT, koriIgnoreFilter);
    while (node = walk.nextNode()) {
        if (missingTranslation = translateContent(node))
            missingTranslations.push(missingTranslation);
    }

    return missingTranslations;
}

function translateContent(node) {
    if (node.nodeName == 'IMG')
        return;

    console.log('registering', node);
    var content = node.nodeName == 'IMG' ? node.src.trim() : node.textContent.trim();
    if (!content)
        return;

    var translation = translations[content];
    if (!translation) {
        console.log('Missing translation for', content);
        return content;
    }

    console.log('Translating', content, 'to', translation);
    node.textContent = translation;
    return;
}

function replaceNode(node, content) {
    if (node.parentElement)
        // replace text with a placeholder
        node.parentElement.innerHTML = '<kori-content text="' + content + '"></kori-content>';
}

async function observeCallback(mutations) {
    console.log("Observe callback", mutations);
    var missingTranslations = [];

    mutations.forEach(function (mutation) {
        if (mutation.target.tagName == "KORI-CONTENT"
            || mutation.target.parentElement?.tagName == "KORI-CONTENT"
            || mutation.target.classList?.contains('kori-ignore')
            || mutation.target.parentElement?.classList.contains('kori-ignore'))
            return;

        var missingTranslation;
        if (mutation.type == 'characterData') {
            console.log('Character data mutation', mutation.target);
            if (missingTranslation = translateContent(mutation.target))
                missingTranslations.push({ n: mutation.target, missingTranslation });
        }
        else if (mutation.type == 'childList') {
            console.log('Mutaton childList', mutation.target);
        }
        else {
            mutation.addedNodes.forEach(registerNodesUnder);
        }
    });

    translations = await dotNet.invokeVoidAsync("AddMissingTranslationsAsync", missingTranslations.map(x => x.missingTranslation));
    for (var i = 0; i < missingTranslations.length; i++) {
        translateContent(missingTranslations[i].n);
    }
}

function getBrowserLanguage() {
    var lang = (navigator.languages && navigator.languages.length) ? navigator.languages[0] :
        navigator.userLanguage || navigator.language || navigator.browserLanguage || 'en';
    return lang.substring(0, 2);
}

