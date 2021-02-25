import { Injectable } from "@angular/core";

@Injectable({
  providedIn: 'root',
})
export class SettingsStateService {

    private issuesEnabled: boolean;

    public getIssue(): boolean {
        return this.issuesEnabled;
    }

    public setIssue(settings: boolean): void {
        this.issuesEnabled = settings;
    }
}