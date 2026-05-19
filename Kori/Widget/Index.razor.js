export function initialize(dotnet) {
    window.addEventListener('message', async (event) => {
        console.log('Received message from parent:', event.data);

        try {
            var data = JSON.parse(event.data);
            if (data?.type == "method") {
                console.log(data.method);
                await dotnet.invokeMethodAsync(data.method);
            }
        } catch (e) {
            console.error('oops', e);
        }
    });
}

export function bold() {
    console.log('bolding');
    window.parent?.postMessage(JSON.stringify({ type: 'format', command: 'bold' }), '*');
}

export function italic() {
    console.log('italicing');
    window.parent?.postMessage(JSON.stringify({ type: 'format', command: 'italic' }), '*');
}