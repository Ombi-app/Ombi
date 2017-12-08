import { IIssueCategory, IMovieRequests, ITvRequests } from "./";

export interface IIssues {
    id?: number;
    subject: string;
    description: string;
    issueCategory: IIssueCategory;
    status: IssueStatus;
    resolvedDate?: Date;
}

export interface ITvIssues extends IIssues {
    tvId: number;
    child: ITvRequests;
}

export interface IMovieIssues extends IIssues {
    movieId: number;
    movie: IMovieRequests;
}

export enum IssueStatus {
    Pending = 0,
    InProgress = 1,
    Resolved = 2,
}
