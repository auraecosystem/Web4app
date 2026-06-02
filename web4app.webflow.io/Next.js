import createMDX from "@next/mdx"
import remarkGfm from "remark-gfm"
import rehypeSlug from "rehype-slug"

const withMDX = createMDX({
  extension: /\.(md|mdx)$/,
  options: {
    remarkPlugins: [remarkGfm],
    rehypePlugins: [rehypeSlug]
  }
})

const nextConfig = {
  pageExtensions: [
    "js",
    "jsx",
    "ts",
    "tsx",
    "md",
    "mdx"
  ]
}

export default withMDX(nextConfig)
