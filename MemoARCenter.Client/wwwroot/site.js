window.triggerInputClick = (inputId) => {
const inputFile = document.getElementById(inputId);
if (inputFile) {
inputFile.click();
}
};

window.resetFileInput = (id) => {
const inputFile = document.getElementById(id);
if (inputFile) {
inputFile.value = ""; // Clear the file input on load
}
};

window.siteJs = {
createObjectUrl: (fileData) => {
// Convert the raw file data into a Blob object
const blob = new Blob([fileData.arrayBuffer], { type: fileData.type });
return URL.createObjectURL(blob); // Create an object URL for the Blob
},
revokeObjectUrl: (url) => {
URL.revokeObjectURL(url); // Revoke the object URL when it's no longer needed
},
getBlobData: async (url) => {
const response = await fetch(url);
const blob = await response.blob();
return new Uint8Array(await blob.arrayBuffer());
},
isIphone: () => {
const userAgent = navigator.userAgent || navigator.vendor || window.opera;
return /iPhone|iPad|iPod|Mac/i.test(userAgent);
}
};
