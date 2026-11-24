using AngleSharp.Dom;

using Bunit;

using Microsoft.AspNetCore.Components;

namespace NetWorthTracker.UI.Tests.TestHelpers;

public static class BunitExtensions
{
    /// <summary>
    ///     Mimics RTL's getByRole(role, { name }). Supports explicit role="..." attributes
    ///     as well as common implicit HTML roles (button, textbox, alert, dialog, heading).
    /// </summary>
    public static IElement GetByRole<TComponent>(
        this IRenderedComponent<TComponent> cut,
        string role,
        string? name = null
    ) where TComponent : IComponent
    {
        var selector = role.ToLowerInvariant() switch
        {
            "button" => "[role='button'], button, input[type='button'], input[type='submit'], input[type='reset']",
            "textbox" =>
                "[role='textbox'], input:not([type]), input[type='text'], input[type='email'], input[type='password'], input[type='search'], input[type='tel'], input[type='url'], textarea",
            "checkbox" => "[role='checkbox'], input[type='checkbox']",
            "radio" => "[role='radio'], input[type='radio']",
            "link" => "[role='link'], a[href]",
            "heading" => "[role='heading'], h1, h2, h3, h4, h5, h6",
            "alert" => "[role='alert']",
            "dialog" => "[role='dialog'], dialog",
            _ => $"[role='{role}']"
        };

        List<IElement> matches = cut.FindAll(selector)
            .Where(el => name is null || MatchesAccessibleName(cut, el, name))
            .ToList();

        return matches.Count switch
        {
            0 => throw new ElementNotFoundException(
                $"Unable to find an accessible element with the role \"{role}\""
                + (name is not null ? $" and name \"{name}\"" : "")
                + "."),
            > 1 => throw new InvalidOperationException(
                $"Found multiple elements with the role \"{role}\""
                + (name is not null ? $" and name \"{name}\"" : "")
                + "."),
            _ => matches[0]
        };
    }

    /// <summary>
    ///     Mimics RTL's getByLabelText(text). Resolves via &lt;label for="..."&gt;, wrapped &lt;label&gt;,
    ///     or aria-label attributes.
    /// </summary>
    public static IElement GetByLabelText<TComponent>(this IRenderedComponent<TComponent> cut, string labelText)
        where TComponent : IComponent
    {
        List<IElement> labels = cut.FindAll("label")
            .Where(l => NormalizeWhitespace(l.TextContent)
                .Contains(NormalizeWhitespace(labelText), StringComparison.OrdinalIgnoreCase))
            .ToList();

        var matches = new List<IElement>();

        foreach (var label in labels)
        {
            var forId = label.GetAttribute("for");
            if (!string.IsNullOrWhiteSpace(forId))
            {
                var target = cut.FindAll($"#{forId}")
                    .FirstOrDefault();
                if (target is not null)
                {
                    matches.Add(target);
                }
            }
            else
            {
                var nestedControl = label.QuerySelector("input, textarea, select");
                if (nestedControl is not null)
                {
                    matches.Add(nestedControl);
                }
            }
        }

        if (matches.Count == 0)
        {
            matches.AddRange(
                cut.FindAll("[aria-label]")
                    .Where(el => el.GetAttribute("aria-label")!.Contains(
                        labelText,
                        StringComparison.OrdinalIgnoreCase)));
        }

        return matches.Count switch
        {
            0 => throw new ElementNotFoundException($"Unable to find a label with the text of: {labelText}"),
            > 1 => throw new InvalidOperationException($"Found multiple form controls matching label: {labelText}"),
            _ => matches[0]
        };
    }

    /// <summary>
    ///     Mimics RTL's getByText(text).
    /// </summary>
    public static IElement GetByText<TComponent>(
        this IRenderedComponent<TComponent> cut,
        string text,
        string selector = "*"
    ) where TComponent : IComponent
    {
        List<IElement> matches = cut.FindAll(selector)
            .Where(el => el.ChildElementCount == 0
                         && el.TextContent.Trim()
                             .Contains(text, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return matches.Count switch
        {
            0 => throw new ElementNotFoundException($"Unable to find an element with the text: {text}"),
            > 1 => throw new InvalidOperationException($"Found multiple elements with the text: {text}"),
            _ => matches[0]
        };
    }

    /// <summary>
    ///     Escape-hatch query matching RTL's getByTestId(id).
    /// </summary>
    public static IElement GetByTestId<TComponent>(this IRenderedComponent<TComponent> cut, string testId)
        where TComponent : IComponent
    {
        return cut.Find($"[data-testid='{testId}']");
    }

    /// <summary>
    ///     Mimics userEvent.clear() + userEvent.type() by dispatching both input and change events.
    /// </summary>
    public static async Task TypeAsync(this IElement element, string value)
    {
        await element.InputAsync(new ChangeEventArgs { Value = value });
        await element.ChangeAsync(new ChangeEventArgs { Value = value });
    }

    private static bool MatchesAccessibleName<TComponent>(
        IRenderedComponent<TComponent> cut,
        IElement element,
        string expectedName
    ) where TComponent : IComponent
    {
        var ariaLabel = element.GetAttribute("aria-label");
        if (!string.IsNullOrWhiteSpace(ariaLabel)
            && ariaLabel.Trim()
                .Equals(expectedName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (element.Id is { Length: > 0 } id)
        {
            var associatedLabel = cut.FindAll($"label[for='{id}']")
                .FirstOrDefault();
            if (associatedLabel is not null
                && associatedLabel.TextContent.Trim()
                    .Equals(expectedName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return element.TextContent.Trim()
            .Equals(expectedName, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeWhitespace(string text)
    {
        return string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }
}
