import { Injectable } from "@angular/core";

@Injectable()
export class StorageService  {

    public get(key: string): string {
        return localStorage.getItem(key);
    }

    public save(key: string, value: string): void {
        this.remove(key);
        localStorage.setItem(key, value);
    }

    public remove(key: string) {
        localStorage.removeItem(key);
    }
}
