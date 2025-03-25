export async function handleResponse(response) {
    const json = await response.json();

    if (response.ok || json.error) return json;

    if (json.errors) {
        return { error: Object.values(response.errors)[0][0] };
    }

    return { error: "Request failed with HTTP code " + response.status };
}

document.querySelectorAll(".logoutContainer").forEach(closeBtn => {
    closeBtn.onclick = () => {
        localStorage.clear();
        window.location.href = "/index.html";
    };
});
