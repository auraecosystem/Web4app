import config from "../../../next.config";

// Get the asset prefix from config
const assetPrefix = config.assetPrefix || config.basePath || '';

export function Icon() {
    return (
        <img
            src={`${assetPrefix}/icons/star.svg`} // Prefix required for CDN caching
            alt="Star icon"
        />
    );
}
