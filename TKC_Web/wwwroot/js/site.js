// ERROR HANDLER SYSTEM
window.globalErrorHandler = {

    register: function (dotNetHelper) {

        // Normal JS Error
        window.addEventListener("error", function (event) {

            const message = 
                event?.error?.message ||
                event?.message ||
                "Unknown error";

            dotNetHelper.invokeMethodAsync(
                "HandlerError",
                message);
        });

        // Promise / async Error
        window.addEventListener("unhandledrejection", function (event) {

            let message = "Unhandled promise rejection";

            if (event.reason)   {

                if (typeof event.reason === "string")
                    message = event.message;

                else if (event.reason.message)
                    message = event.reason.message;
            }

            dotNetHelper.invokeMethodAsync(
                "HandleError",
                message);
        });

        // Blazor Disconnet / WASM Error
        /*window.Blazor.defaultReconnectionHandler._reconnectCallback = function () {
            
            dotNetHelper.invokeMethodAsync(
                "HandleDisconnect");
        };*/

        // Network observer
        window.addEventListener("offline", function () {

            dotNetHelper.invokeMethodAsync(
                "HandleDisconnect");
        });

        window.addEventListener("online", function () {
            dotNetHelper.invokeMethodAsync(
            "HandleReconnect");
        });
    }
};

// DOWNLOAD SYSTEM
window.downloadFile = (fileName, bytesBase64) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = "data:application/octet-stream;base64," + bytesBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

window.downloadFileFromApi = async (url, token) => {

    const response = await fetch(url, {
        headers: {
            "Authorization": "Bearer " + token
        }
    });

    if (!response.ok) {
        console.error("Download failed", response.status);
        return;
    }

    // Dateiname aus Header holen
    const disposition = response.headers.get("content-disposition");
    let fileName = "file";

    if (disposition) {
        // zuerst filename*=UTF-8'' prüfen
        let filenameStarMatch = disposition.match(/filename\*\s*=\s*UTF-8''([^;]+)/i);
        if (filenameStarMatch && filenameStarMatch[1]) {
            fileName = decodeURIComponent(filenameStarMatch[1]);
        } else {
            // fallback: normales filename=
            let filenameMatch = disposition.match(/filename\s*=\s*"?(.*?)"?($|;)/i);
            if (filenameMatch && filenameMatch[1]) {
                fileName = filenameMatch[1];
            }
        }
    }

    const blob = await response.blob();

    const urlBlob = URL.createObjectURL(blob);

    const link = document.createElement("a");
    link.href = urlBlob;
    link.download = fileName;

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    URL.revokeObjectURL(urlBlob);
};