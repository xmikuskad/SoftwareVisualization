using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

[Serializable]
public class VerticeData
{
    public long id { get; set; }
    public string text { get; set; }
    public string title { get; set; }
    public VerticeType verticeType { get; set; }

    // attributes
    public string eid { get; set; }
    public string name { get; set; }
    public string description { get; set; }
    public string comment { get; set; }
    public string message { get; set; }
    public string[] identities { get; set; }
    public string[] emails { get; set; }
    public string[] roles { get; set; }
    public string[] author { get; set; }
    public DateTime? created { get; set; }
    public string url { get; set; }
    public long number { get; set; }
    public string[] assignee { get; set; }
    public string[] type { get; set; }
    public string[] priority { get; set; }
    public string[] severity { get; set; }
    public DateTime? start { get; set; }
    public DateTime? due { get; set; }
    public string[] iteration { get; set; }
    public string[] status { get; set; }
    public string[] resolution { get; set; }
    public float estimate { get; set; }
    public float spent { get; set; }
    public long progress { get; set; }
    public string[] categories { get; set; }
    public string identifier { get; set; }
    public DateTime? committed { get; set; }
    public string changes { get; set; }
    public string[] branches { get; set; }
    public string[] tags { get; set; }
    public string[] mime { get; set; }
    public long size { get; set; }
    //public string relation { get; set; } Only in edges
    public string[] roleClasses { get; set; }
    public string[] roleSuperClasses { get; set; }
    public string[] typeClass { get; set; }
    public string[] priorityClass { get; set; }
    public string[] prioritySuperClass { get; set; }
    public string[] severityClass { get; set; }
    public string[] severitySuperClass { get; set; }
    public string[] statusClass { get; set; }
    public string[] statusSuperClass { get; set; }
    public string[] resolutionClass { get; set; }
    public string[] resolutionSuperClass { get; set; }

    public DateTime? begin { get; set; }

    // This is used to instantiate a "fake" person for tasks we dont have authorship for
    public VerticeData(long id)
    {
        this.id = id;
    }

    public VerticeData(RawVerticeData rawVerticeData)
    {
        // System.Console.WriteLine(rawVerticeData.attributes["Created"]?.ToString());
        // System.Console.WriteLine(rawVerticeData.ToString());
        this.id = rawVerticeData.id;
        this.text = rawVerticeData.text;
        this.title = rawVerticeData.title;
        this.verticeType = (VerticeType)System.Enum.Parse(typeof(VerticeType), rawVerticeData.archetype);
        foreach (string key in rawVerticeData.attributes.Keys)
        {
            PropertyInfo info = typeof(VerticeData).GetProperty(DataUtils.GetFormattedPropertyName(key));
            Type foundType = info.PropertyType;
            if (foundType != typeof(string))
            {
                if (foundType == typeof(string).MakeArrayType())
                {
                    info.SetValue(this, ((Newtonsoft.Json.Linq.JArray)rawVerticeData.attributes[key]).ToObject<String[]>(), null);
                }
                else if (foundType == typeof(long))
                {
                    info.SetValue(this, long.Parse((String)rawVerticeData.attributes[key]), null);
                }
                else if (foundType == typeof(float))
                {
                    info.SetValue(this, float.Parse((String)rawVerticeData.attributes[key], CultureInfo.InvariantCulture), null);
                }
                else if (foundType == typeof(DateTime) || foundType == typeof(DateTime?))
                {
                    string format = "yyyy-MM-dd HH:mm:ss";
                    info.SetValue(this, DateTime.ParseExact((String)rawVerticeData.attributes[key], format, CultureInfo.InvariantCulture, DateTimeStyles.None), null);
                }
            }
            else
            {
                info.SetValue(this, rawVerticeData.attributes[key], null);
            }
        }
    }

    public override string ToString()
    {
        return $"Vertice Data {nameof(id)}: {id}, {nameof(text)}: {text}, {nameof(title)}: {title}, " +
               $"{nameof(verticeType)}: <b>{verticeType}</b>, {nameof(eid)}: {eid}, {nameof(name)}: {name}, " +
               $"{nameof(description)}: {description}, {nameof(comment)}: {comment}, {nameof(message)}: {message}, " +
               $"{nameof(identities)}: {identities}, {nameof(emails)}: {emails}, {nameof(roles)}: {roles}, " +
               $"{nameof(author)}: {author}, {nameof(created)}: {created}, {nameof(url)}: {url}, " +
               $"{nameof(number)}: {number}, {nameof(assignee)}: {assignee}, {nameof(type)}: {type}, " +
               $"{nameof(priority)}: {priority}, {nameof(severity)}: {severity}, {nameof(start)}: {start}, " +
               $"{nameof(due)}: {due}, {nameof(iteration)}: {iteration}, {nameof(status)}: {status}, " +
               $"{nameof(resolution)}: {resolution}, {nameof(estimate)}: {estimate}, {nameof(spent)}: {spent}, " +
               $"{nameof(progress)}: {progress}, {nameof(categories)}: {categories}, {nameof(identifier)}: {identifier}, " +
               $"{nameof(committed)}: {committed}, {nameof(changes)}: {changes}, {nameof(branches)}: {branches}," +
               $" {nameof(tags)}: {tags}, {nameof(mime)}: {mime}, {nameof(size)}: {size}, " +
               $"{nameof(roleClasses)}: {roleClasses}, {nameof(roleSuperClasses)}: {roleSuperClasses}, " +
               $"{nameof(typeClass)}: {typeClass}, {nameof(priorityClass)}: {priorityClass}, " +
               $"{nameof(prioritySuperClass)}: {prioritySuperClass}, {nameof(severityClass)}: {severityClass}, " +
               $"{nameof(severitySuperClass)}: {severitySuperClass}, {nameof(statusClass)}: {statusClass}, " +
               $"{nameof(statusSuperClass)}: {statusSuperClass}, {nameof(resolutionClass)}: {resolutionClass}, " +
               $"{nameof(resolutionSuperClass)}: {resolutionSuperClass}";
    }

    public DateTime GetTime()
    {
        return this.created ?? this.begin ?? DateTime.MinValue;
    }

    public DateTime GetTimeWithoutHours()
    {
        return GetTime().Date;
    }

    public bool HasDate(DateTime date)
    {
        return GetTime() == date;
    }

    public bool HasDateWithoutHours(DateTime date)
    {
        return GetTimeWithoutHours() == date;
    }

    public bool HasDatesWithoutHours(List<DateTime> dates)
    {
        return dates.Contains(GetTimeWithoutHours());
    }

    public bool IsDateBetween(DateTime from, DateTime to)
    {
        return from <= GetTimeWithoutHours() && GetTimeWithoutHours() <= to;
    }
}