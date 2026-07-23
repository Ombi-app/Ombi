import { describe, expect, it, vi } from "vitest";
import { of } from "rxjs";
import { EpisodeRequestComponent } from "./episode-request.component";

function createComponent(isAdmin: boolean, canSelectQualityProfile: boolean, dialogResult: any = null, requestResult: any = { result: true }) {
    const dialogRef = { close: vi.fn() };
    const requestService = { requestTv: vi.fn().mockReturnValue(of(requestResult)) };
    const notificationService = { send: vi.fn(), sendRequestEngineResultError: vi.fn() };
    const dialog = { open: vi.fn().mockReturnValue({ afterClosed: () => of(dialogResult) }) };
    const translate = { instant: vi.fn((key: string) => key), currentLang: "en" };
    const auth = { hasRole: vi.fn().mockReturnValue(canSelectQualityProfile) };
    const data = {
        isAdmin,
        requestOnBehalf: undefined,
        series: {
            id: 42,
            title: "Show",
            firstSeason: false,
            latestSeason: false,
            requestAll: false,
            requested: false,
            seasonRequests: [{ seasonNumber: 1, episodes: [{ episodeNumber: 1, selected: true, requested: false }] }],
        },
    };

    const component = new EpisodeRequestComponent(
        dialogRef as any,
        data as any,
        requestService as any,
        notificationService as any,
        dialog as any,
        translate as any,
        auth as any,
    );

    return { component, data, dialog, requestService };
}

describe("EpisodeRequestComponent quality profile selection", () => {
    it("opens a quality-only dialog for a normal user with the dedicated role", async () => {
        const { component, dialog, requestService } = createComponent(false, true, { sonarrPathId: 7 });

        await component.submitRequests();

        expect(dialog.open).toHaveBeenCalledWith(
            expect.anything(),
            expect.objectContaining({ data: expect.objectContaining({ qualityOnly: true }) }),
        );
        await vi.waitFor(() => expect(requestService.requestTv).toHaveBeenCalledWith(
            expect.objectContaining({
                qualityPathOverride: 7,
                requestOnBehalf: undefined,
                rootFolderOverride: undefined,
                languageProfile: undefined,
            }),
        ));
    });

    it("keeps the full advanced dialog for admins", async () => {
        const { component, dialog } = createComponent(true, false);

        await component.submitRequests();

        expect(dialog.open).toHaveBeenCalledWith(
            expect.anything(),
            expect.objectContaining({ data: expect.objectContaining({ qualityOnly: false }) }),
        );
    });

    it("does not mark the series or episodes requested when the dialog is cancelled", async () => {
        const { component, data, requestService } = createComponent(false, true, null);

        await component.submitRequests();

        expect(requestService.requestTv).not.toHaveBeenCalled();
        expect(data.series.requested).toBe(false);
        expect(data.series.seasonRequests[0].episodes[0].requested).toBe(false);
    });

    it("marks the series and episodes requested after a successful submission", async () => {
        const { component, data, requestService } = createComponent(false, true, { sonarrPathId: 7 });

        await component.submitRequests();

        await vi.waitFor(() => expect(requestService.requestTv).toHaveBeenCalled());
        expect(data.series.requested).toBe(true);
        expect(data.series.seasonRequests[0].episodes[0].requested).toBe(true);
    });

    it("does not mark the series or episodes requested when submission fails", async () => {
        const { component, data, requestService } = createComponent(false, true, { sonarrPathId: 7 }, { result: false });

        await component.submitRequests();

        await vi.waitFor(() => expect(requestService.requestTv).toHaveBeenCalled());
        expect(data.series.requested).toBe(false);
        expect(data.series.seasonRequests[0].episodes[0].requested).toBe(false);
    });

    it("requests directly for a normal user without the dedicated role", async () => {
        const { component, dialog, requestService } = createComponent(false, false);

        await component.submitRequests();

        expect(dialog.open).not.toHaveBeenCalled();
        expect(requestService.requestTv).toHaveBeenCalled();
    });
});