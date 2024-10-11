let translationCache = {};
let dotNet = {};
let app = {};
let observer = {};
let language = getBrowserLanguage();
var koriAuthorized = false;
let topBar = {};
let activeNode = null, activeMessageId = null;

let koriIgnoreFilter = function (node) {
    var approvedNodes = ['#text', 'IMG'];

    if (!approvedNodes.includes(node.nodeName) || node.parentNode.nodeName == 'SCRIPT' || node.koriTranslated == language)
        return NodeFilter.FILTER_SKIP;

    var closest = node.parentElement.closest('.kori-ignore');
    if (closest)
        return NodeFilter.FILTER_SKIP;

    return NodeFilter.FILTER_ACCEPT;
}

function init(targetElementId, selectedLanguage, dotNetObjectReference, serverTranslationCache) {
    language = selectedLanguage;
    dotNet = dotNetObjectReference;

    buildTranslationCache(serverTranslationCache);

    initKoriElement(targetElementId);

    window.addEventListener("click", e => {
        e.stopImmediatePropagation();
        mouseClickHandler(e);
    });

    initKoriTopBar();
}

function buildTranslationCache(serverTranslationCache) {
    if (serverTranslationCache) {
        translationCache = serverTranslationCache;
        for (let key in translationCache)        
            translationCache[key].Nodes = [];
    }
    else {
        for (let key in translationCache) {
            translationCache[key].Submitted = false;
            translationCache[key].Translation = null;
            translationCache[key].Nodes = [];
        }
    }

    console.log('Kori translation cache initialized from Ibis, ', translationCache);
}

function initKoriElement(targetElementId) {
    if (/complete|interactive|loaded/.test(document.readyState)) {
        initElement(targetElementId);
    } else {
        window.addEventListener('DOMContentLoaded', () => initElement(targetElementId));
    }
}

function initKoriTopBar() {
    topBar = document.getElementById("kori-top-bar");   

    console.log('Kori top bar initialized.');
}

function initElement(targetElementId) {
    console.log("Initializing element");

    app = document.getElementById(targetElementId);

    registerNodesUnder(app);
    translateNodes();

    observer = new MutationObserver(observeCallback);
    observer.observe(app, { childList: true, characterData: true, subtree: true });

    console.log('Observer registered for ' + targetElementId + '.');
}

function registerNodesUnder(el) {
    var n, walk = document.createTreeWalker(el, NodeFilter.SHOW_TEXT | NodeFilter.SHOW_ELEMENT, koriIgnoreFilter);
    while (n = walk.nextNode()){
        console.log('Registering node', n.nodeName);
        registerNode(n);
    }
}

function registerNode(node) {
    if (node.koriRegistered == language || node.koriTranslated == language)
        return;

    var content = node.nodeName == 'IMG' ? node.src.trim() : node.textContent.trim();
    //console.log("content in registerNode: ", content);

    var tag = getTagContent(node) ?? (node.koriContent ?? content.trim());
    //console.log("tag in registerNode: ", tag);

    if (!tag)
        return;

    node.koriRegistered = language;
    node.koriContent = tag;
    node.parentElement?.classList.add('kori-initializing');

    if (tag in translationCache && translationCache[tag].Nodes.indexOf(node) < 0) {
        translationCache[tag].Nodes.push(node);

        if (translationCache[tag].id !== undefined && node.nodeName == '#text') {
            node.parentElement.setAttribute('kori-id', translationCache[tag].id);
        }

    } else {
        translationCache[tag] = {
            Nodes: [node],
            Translation: null
        };
    }
}

function getTagContent(node) {
    var tagContent = node.parentElement?.getAttribute('data-tag');
    if (tagContent) {
        return tagContent.trim();
    }
    return null;
}

function observeCallback(mutations) {
    console.log("Observe callback");

    mutations.forEach(function (mutation) {
        if (mutation.target.classList?.contains('kori-ignore') || mutation.target.parentElement?.classList.contains('kori-ignore'))
            return;        

        if (mutation.type == 'characterData')
            registerNode(mutation.target, NodeType.TEXT);
        else
            mutation.addedNodes.forEach(registerNodesUnder);

        translateNodes();
    });
}

function translateNodes() {
    console.log('translateNodes');

    var contentToTranslate = {};

    for (let key in translationCache) {
        if (!translationCache[key].Submitted && !translationCache[key].Translation) {
            translationCache[key].Submitted = true;

            let tag = key;

            translationCache[key].Nodes.forEach(node => {
                let text = node.textContent || "";                 

                if (isPlaceholder(text)) {
                    contentToTranslate[tag] = "";                    
                } else {
                    if (text.length > 0) {
                        contentToTranslate[tag] = text; 
                    }
                }
            });
        }
    }    

    dotNet.invokeMethodAsync("TranslateAsync", contentToTranslate).then(translations => {
        console.log('Received new translations from Ibis.', translations);

        for (var key in translations) {
            if (translations[key] === "") {
                translations[key] = " ";
            }
            translationCache[key].Translation = translations[key];
        }

        replaceWithTranslatedText();
    });
}

//Function to check if text is a placeholder
function isPlaceholder(text) {
    const placeholders = [
        "Type your title here",
        "Author Name",
        "Type blog post content here." 
    ];

    return placeholders.includes(text);
}

function replaceWithTranslatedText() {
    observer.disconnect();

    console.log('replaceWithTranslatedText - translationCache', translationCache);

    for (let key in translationCache) {
        var translation = translationCache[key];

        console.log('translation', translation);

        if (!translation.Translation)
            continue;

        for (let node of translation.Nodes) {
            // if the node is an img, replace the src attribute
            if (node.nodeName == 'IMG') {
                node.src = translation.Translation;
                node.koriTranslated = language;
            } else if (node.textContent != translation.Translation) {

                if (translation.text != undefined) {
                    node.textContent = translation.text || "";
                }
                node.koriTranslated = language;
            }

            node.parentElement?.classList.remove('kori-initializing');
            node.parentElement?.classList.add('kori-enabled');

            if (node.textContent.trim() == "") {
                node.parentElement?.classList.add('empty-content');
            }

            if (node.nodeName == '#text' && translation.text && node.parentElement) {
                node.parentElement.innerHTML = translation.html;
            }
        }
    }

    console.log('Translated page from Ibis and enabled Kori top bar.');

    observer.observe(app, { childList: true, characterData: true, subtree: true });
}

function getBrowserLanguage() {
    var lang = (navigator.languages && navigator.languages.length) ? navigator.languages[0] :
        navigator.userLanguage || navigator.language || navigator.browserLanguage || 'en';
    return lang.substring(0, 2);
}

let playAudio = function (url) {
    const sound = new Howl({
        src: [url]
    });
    sound.play();
}

// mouse click handler for kori widget and elements
function mouseClickHandler(e) {
    var t = e.target;

    // click login menu
    if (t.closest(".kori-login__btn")) {
        koriAuthorized = true;
        if (koriAuthorized) {
            document.getElementById("kori-login").classList.remove("show");
            document.body.classList.add("kori-loggedin"); // add the class to <body>            
        }
    }

    if (koriAuthorized) {
        // click kori enabled elements
        toggleSelected(t);        
    } else {
        console.log("please login to use kori services");
        return;
    }
}

// selecting and unselecting kori-enabled elements
function toggleSelected(t) {
    document.getElementsByClassName("selected")[0]?.classList.remove("selected");
    document.getElementsByClassName("show")[0]?.classList.remove("show");

    var topBar = document.getElementById('kori-top-bar');

    var koriElem = t.closest('.kori-enabled');
    console.log('koriElem', koriElem);
    if (!koriElem) {
        // clicked outside of all kori elements
        document.getElementsByClassName("show")[0]?.classList.remove("show");

        if (!topBar.classList.contains("show")) {
            document.body.style.marginTop = '0';

            var buttons = topBar.querySelectorAll('.top-bar-button');
            buttons.forEach(function (button) {
                if (!button.classList.contains('toggle-button')) {
                    var iconImg = button.querySelector('img');
                    if (iconImg) {
                        iconImg.src = iconImg.src.replace('-selected.svg', '.svg');
                    }
                }
            });
        }          

        if (activeMessageId) {
            cancelEdit();
        }

        activeNode = null;
        return;
    }

    if (!koriElem.classList.contains("selected")) {
        koriElem.classList.add("selected");
        document.getElementsByClassName("show")[0]?.classList.remove("show");
        toggleTopBar(koriElem);
    }
}

// showing and hiding kori top bar
function toggleTopBar(t) {  
    var topBar = document.getElementById("kori-top-bar");

    t.appendChild(topBar);

    topBar.classList.add("show");

    if (topBar.classList.contains("show")) {
        // adjusts the top margin to match the top-bar height
        document.body.style.marginTop = '100px';
    }

    const koriId = t.getAttribute('kori-id');
    // search for matching node in translation cache
    for (let key in translationCache) {
        for (var i = 0; i < translationCache[key].Nodes.length; i++)

            if (t.contains(translationCache[key].Nodes[i])) {
                activeNode = translationCache[key].Nodes[i];
                activeMessageId = key;
                break;
            }

        if (koriId != null && koriId == translationCache[key].id) {
            activeNode = t;
            activeMessageId = key;
            break;
        }
    }

    console.log('Set active node', activeNode);
}

function getActiveImageSrc() {
    if (activeNode && activeNode.tagName === 'IMG') {
        return activeNode.src;
        //console.log('Active node is an image', activeNode.src);
    }

    //console.log('Active node is not an image', activeNode)
    return null;
}

function edit() {
    if (!activeNode) {
        //console.log('Unable to edit element', activeNode);
        return;
    }

    var translation = translationCache[activeMessageId];

    if (isTranslationAlreadySaved(translation)) {
        var activeNodeParent = getActiveNodeParentByKoriId(translation);
        activateNodeEdition(activeNodeParent);
        replaceInnerHtmlBeforeTopBar(activeNodeParent, getTranslationRawMarkdownText(translation));
    }
    else {
        var parentElement = activeNode.parentElement;
        activateNodeEdition(parentElement);

        // If the Translation is empty or contains only whitespace
        if (!translation || !translation.Translation || translation.Translation.trim() === "") {

            // Insert a temporary space to ensure the cursor appears
            activeNode.textContent = "\u200B"; // Zero-width space character
        } else {
            activeNode.textContent = getTranslationRawMarkdownText(translation);
        }
    }

    document.getElementById("kori-top-bar").contentEditable = "false";
}
function getTranslationRawMarkdownText(translation) {
    return translation.text ?? translation.Translation;
}

function activateNodeEdition(node) {
    node.classList.add('kori-ignore');
    node.contentEditable = "true";
    node.focus();
}

function deactivateNodeEdition(node, translation) {
    node.contentEditable = "false";
    node.classList.remove('kori-ignore');
    node.classList.remove('selected');

    resetTopBar();

    node.innerHTML = translation.html;
}

function getActiveNodeParentByKoriId(translation) {
    return document.querySelector(`[kori-id="${translation.id}"]`);
}

function isTranslationAlreadySaved(translation) {
    return translation.id;
}

//function replaceInnerHtmlBeforeWidget(node, markdownTxt) {
//    while (node.firstChild) {
//        if (node.firstChild.id !== "kori-widget") {
//            node.removeChild(node.firstChild);
//        } else {
//            break;
//        }
//    }

//    node.firstChild.insertAdjacentHTML('beforebegin', markdownTxt);
//}

function replaceInnerHtmlBeforeTopBar(node, markdownTxt) {
    while (node.firstChild) {
        //console.log("-------------node.firstChild", node.firstChild)
        if (node.firstChild.id !== "kori-top-bar") {
            node.removeChild(node.firstChild);
        } else {
            break;
        }
    }

    node.firstChild.insertAdjacentHTML('beforebegin', markdownTxt);
}

function editImage() {
    console.log("Entered the edit image function");
}

function showSidebar() {
    document.body.style.marginRight = "317px";

    var topBar = document.getElementById('kori-top-bar');
    topBar.style.width = "calc(100% - 317px)";
}

function hideSidebar() {
    document.body.style.marginRight = "0px";

    var topBar = document.getElementById('kori-top-bar');
    topBar.style.width = "100%";
}

function cancelEdit() {
    var translation = translationCache[activeMessageId];

    if (isTranslationAlreadySaved(translation)) {
        var activeNodeParent = document.querySelector(`[kori-id="${translation.id}"]`);
        deactivateNodeEdition(activeNodeParent, translation);
    } else {
        activeNode.textContent = translation.Translation;
        activeNode.parentElement.contentEditable = "false";
        activeNode.parentElement.classList.remove('selected');
    }
    hideSidebar();
}

function closeSearch() {
    activeNode.parentElement.contentEditable = "true";
    activeNode.parentElement.classList.add('selected'); 

    hideSidebar();

    var topBar = document.getElementById('kori-top-bar');
    if (topBar) {
        var selectedButton = topBar.querySelector('.top-bar-button.selected');
        if (selectedButton) {          
            selectedButton.classList.remove('selected');
        }
    }
}

function getActiveNodeTextContent(translation) {
    var activeNodeParent = document.querySelector(`[kori-id="${translation.id}"]`);

    var copyNode = activeNodeParent.cloneNode(true);
    var koriTopBar = copyNode.querySelector('#kori-top-bar');    

    return copyNode.textContent.replace(koriTopBar.textContent, '');
}

// Here we just need to make sure that we are updating img src in the same way as
// we are updating text content
function save() {
    if (!activeNode)
        return;

    var translation = translationCache[activeMessageId];
    //console.log("translation: ", translation);
    var textContent = activeNode.textContent;

    if (isTranslationAlreadySaved(translation)) {
        textContent = getActiveNodeTextContent(translation);
    }

    dotNet.invokeMethodAsync("BackToEditAsync").then(r => {

        dotNet.invokeMethodAsync("SaveAsync", activeMessageId, textContent).then(content => {
            console.log('Saved new content to Ibis.', content);

            translationCache[activeMessageId].Translation = content.text;
            translationCache[activeMessageId].text = content.text;
            translationCache[activeMessageId].html = content.html;

            activeNode.parentElement.contentEditable = "false";
            activeNode.parentElement.classList.remove('kori-ignore');            

            if (translation.id) {

                var activeNodeParent = document.querySelector(`[kori-id="${translation.id}"]`);
                deactivateNodeEdition(activeNodeParent, translation);

            } else {
                translationCache[activeMessageId].id = content.id;
                activeNode.parentElement?.setAttribute('kori-id', content.id);
            }

        });
    });
}

// function to check if an element is a descendant of an element with a specific class
function isDescendantOfClass(element, className) {
    while (element) {
        if (element.classList && element.classList.contains(className)) {
            return true;
        }
        element = element.parentElement;
    }
    return false;
}

function checkSelectedContentType() {
    var selectedElement = document.getElementsByClassName("selected")[0];

    if (!selectedElement) {
        return "none";
    }

    if (selectedElement.tagName.toLowerCase() === 'img') {
        return "image";
    }

    // checks if the selected element has the classes 'kori-enabled' and 'selected'
    if (selectedElement.classList.contains('kori-enabled') && selectedElement.classList.contains('selected')) {
        var imgChildren = selectedElement.querySelectorAll('img');

        // Iterates over the child images to check for the presence of 'kori-ignore'
        for (var img of imgChildren) {
            // checks if the image is not inside a parent element with class 'kori-ignore'
            if (!isDescendantOfClass(img, 'kori-ignore')) {
                return "image";
            }
        }
    }

    // if it is not an image, assume it is text
    return "text";
}

// show and hide translation menu
function toggleTranslation(isOpen) {
    console.log("opening translation menu");
    var translation = document.getElementById("kori-translation");
    var widgetActions = document.getElementById("kori-widget__actions");

    if (!translation.classList.contains("show") && isOpen == true) {
        widgetActions.classList.remove("show");
        translation.classList.add("show");
        widgetActions.classList.remove("show");
    }

    if (translation.classList.contains("show") && isOpen == false) {
        translation.classList.remove("show");
        widgetActions.classList.add("show");
    }
}

// show and hide search/content navigation menu

function toggleSearch(isOpen) {
    //console.log("opening search menu");
    var search = document.getElementById("kori-search");
    var widgetActions = document.getElementById("kori-widget__actions");

    if (!search.classList.contains("show") && isOpen == true) {
        widgetActions.classList.remove("show");
        search.classList.add("show");
        widgetActions.classList.remove("show");
    }

    if (search.classList.contains("show") && isOpen == false) {
        search.classList.remove("show");
        widgetActions.classList.add("show");
    }
}

// login to use kori services
function login() {
    console.log("logging in...");
}

function resetTopBar() {
    var topBar = document.getElementById("kori-top-bar");

    // Move the top bar back to the body root
    document.body.appendChild(topBar);

    // Hide the top bar
    topBar.style.display = 'none';
}

function applyMarkdown(symbol) {
    const selectedText = window.getSelection().toString();
    if (selectedText) {
        const newText = symbol + selectedText + symbol;
        document.execCommand('insertText', false, newText);
    }
}

function updateImageSrc(currentSrc, newSrc) {
    var img = document.querySelector(`img[src="${currentSrc}"]`);

    if (img) {
        img.src = newSrc;
        translationCache[activeMessageId].text = newSrc;
        console.log("Image src updated", img);
    }
}



export { init, replaceWithTranslatedText, getBrowserLanguage, playAudio, edit, cancelEdit, save, checkSelectedContentType, editImage, applyMarkdown, getActiveImageSrc, updateImageSrc, showSidebar, closeSearch };
