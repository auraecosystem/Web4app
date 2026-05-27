import tkinter as tk
from tkinter import scrolledtext, filedialog
import markdown
import webbrowser
import os

# ==========================================
# MARKDOWN ENGINE
# ==========================================

def convert_markdown_to_html(md_text: str) -> str:
    return markdown.markdown(md_text)

# ==========================================
# SAVE HTML FILE
# ==========================================

def save_html(content: str):
    file_path = filedialog.asksaveasfilename(
        defaultextension=".html",
        filetypes=[("HTML files", "*.html")]
    )

    if file_path:
        with open(file_path, "w", encoding="utf-8") as f:
            f.write(content)

        webbrowser.open("file://" + os.path.abspath(file_path))

# ==========================================
# GUI APP
# ==========================================

class MarkdownApp:

    def __init__(self, root):
        self.root = root
        self.root.title("Python Markdown Editor")

        # Input box
        self.input_box = scrolledtext.ScrolledText(
            root,
            wrap=tk.WORD,
            width=60,
            height=20
        )
        self.input_box.pack(padx=10, pady=10)

        # Buttons
        btn_frame = tk.Frame(root)
        btn_frame.pack()

        tk.Button(
            btn_frame,
            text="Render HTML",
            command=self.render
        ).pack(side=tk.LEFT, padx=5)

        tk.Button(
            btn_frame,
            text="Save HTML",
            command=self.save
        ).pack(side=tk.LEFT, padx=5)

        # Output preview
        self.output_box = scrolledtext.ScrolledText(
            root,
            wrap=tk.WORD,
            width=60,
            height=15,
            bg="#f0f0f0"
        )
        self.output_box.pack(padx=10, pady=10)

    # ======================================
    # RENDER MARKDOWN
    # ======================================

    def render(self):

        md_text = self.input_box.get("1.0", tk.END)

        html = convert_markdown_to_html(md_text)

        self.output_box.delete("1.0", tk.END)
        self.output_box.insert(tk.END, html)

    # ======================================
    # SAVE OUTPUT
    # ======================================

    def save(self):

        md_text = self.input_box.get("1.0", tk.END)

        html = convert_markdown_to_html(md_text)

        save_html(html)

# ==========================================
# RUN APP
# ==========================================

if __name__ == "__main__":

    root = tk.Tk()

    app = MarkdownApp(root)

    root.mainloop()
