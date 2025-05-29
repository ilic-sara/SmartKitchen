function getApiBaseUrl() {
    return window.apiBaseUrl + '/api/';
}
window.apiClient = {
    get: async (url, includeCredentials = false) => {
        const response = await fetch(getApiBaseUrl() + url, {
            method: 'GET',
            credentials: includeCredentials ? 'include' : 'same-origin'
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`GET request failed: ${errorText}`);
        }

        return await response.json();
    },

    post: async (url, dataObject) => {
        const response = await fetch(getApiBaseUrl() + url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dataObject),
            credentials: 'include'
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`POST request failed: ${errorText}`);
        }

        return await response.json();
    },

    put: async (url, dataObject) => {
        const response = await fetch(getApiBaseUrl() + url, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(dataObject),
            credentials: 'include'
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`PUT request failed: ${errorText}`);
        }
    },

    delete: async (url) => {
        const response = await fetch(getApiBaseUrl() + url, {
            method: 'DELETE',
            credentials: 'include'
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`DELETE request failed: ${errorText}`);
        }
    },
    uploadImage: async function (base64Data, fileName, contentType) {
        const binary = atob(base64Data);
        const byteArray = Uint8Array.from(binary, char => char.charCodeAt(0));
        const blob = new Blob([byteArray], { type: contentType });

        const formData = new FormData();
        formData.append("file", blob, fileName);

        const response = await fetch(getApiBaseUrl() + "FileUpload", {
            method: "POST",
            credentials: "include",
            body: formData
        });

        if (!response.ok) {
            const message = await response.text();
            throw new Error(`Upload failed: ${message}`);
        }

        const result = await response.json();
        return result.url;
    }
};
