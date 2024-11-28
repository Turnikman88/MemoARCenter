// Set data for drag-and-drop
window.setDragData = (event, data) => {
    event.dataTransfer.setData("text/plain", data);
};

// Get data for drag-and-drop
window.getDragData = (event) => {
    return event.dataTransfer.getData("text/plain");
};
