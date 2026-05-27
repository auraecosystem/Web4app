import config from "../next.config";

// Get the base path from config
const basePath = config.basePath || '';

export async function fetchData() {
const response = await fetch(`${basePath}/api/data`);
return response.json();
}
