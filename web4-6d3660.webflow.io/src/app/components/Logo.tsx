import Image from "next/image";

export function Logo() {
    return (
        <Image
            src="/images/logo.png" // No prefix needed - automatically optimized
            alt="Logo"
            width={180}
            height={40}
            priority
        />
    );
}
