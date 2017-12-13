import { IIssueCategory, IMovieRequests, ITvRequests, IUser } from "./";

export interface IIssues {
    id?: number;
    subject: string;
    description: string;
    issueCategory: IIssueCategory;
    issueCategoryId: number;
    status: IssueStatus;
    resolvedDate?: Date;
    comments: IIssueComments[];
}

export interface IIssueDetails extends IIssues {
    child: ITvRequests | undefined;
    movie: IMovieRequests | undefined;
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

export interface IIssueComments {
    userId: string;
    comment: string;
    movieIssueId: number | undefined;
    tvIssueId: number | undefined;
    date: Date;
    user: IUser;
    movieIssues: IMovieIssues | undefined;
    tvIssues: ITvIssues | undefined;
}

export interface IIssuesChat {
    comment: string;
    date: Date;
    username: string;
    adminComment: boolean;
}

export interface INewIssueComments {
    comment: string;
    movieIssueId: number | undefined;
    tvIssueId: number | undefined;
}
