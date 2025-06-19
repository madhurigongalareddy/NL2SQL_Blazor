// Minimal JS helper for file download
window.downloadFileFromBase64 = (fileName, base64Data) => {
    const link = document.createElement('a');
    link.download = fileName;
    link.href = 'data:text/plain;base64,' + base64Data;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};
