import sys
import pdfplumber
import fitz # PyMuPDF
import io

def debug_pdf(path):
    print(f"Opening {path}...")
    try:
        with open(path, "rb") as f:
            content = f.read()
            print(f"Read {len(content)} bytes")
            
            # Test pdfplumber
            print("\n--- PDFPLUMBER ---")
            with pdfplumber.open(io.BytesIO(content)) as pdf:
                text = ""
                for page in pdf.pages:
                    text += page.extract_text() or ""
                print(f"Extracted {len(text)} characters")
                if text.strip():
                    print(f"First 100 chars: {text.strip()[:100]}")

            # Test PyMuPDF
            print("\n--- PYMUPDF ---")
            with fitz.open(stream=content, filetype="pdf") as doc:
                text = ""
                for page in doc:
                    text += page.get_text()
                print(f"Extracted {len(text)} characters")
                if text.strip():
                    print(f"First 100 chars: {text.strip()[:100]}")
    except Exception as e:
        print(f"ERROR: {str(e)}")

if __name__ == "__main__":
    if len(sys.argv) > 1:
        debug_pdf(sys.argv[1])
    else:
        print("Please provide a path to a PDF file")
