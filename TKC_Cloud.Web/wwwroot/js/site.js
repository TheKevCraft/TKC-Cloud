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