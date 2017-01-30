#include <ctime>
#include <cmath>
#include <fstream>
#include <iomanip>
#include <iostream>
#include <sstream>
#include <string>
#include <vector>
#include <map>

/*
 * TODO:
 * - Filter by category (with a hint to what was filtered out [typo within category])
 * - Accumulate the entries of several files into one vector.
 * - print one sheet per month
 * - print a summary: work hours per day / time per category
 * - read from stdin and write to file
 */

static const std::string dateFormat = "%d.%m.%Y";
static const std::string timeFormat = "%H:%M";
static const std::string dateTimeFormat = timeFormat + " " + dateFormat;
static const std::string entrySeparator = "---";

double timeToHours(std::time_t time)
{
   return time / 60.0 / 60.0;
}

std::time_t timeSinceEpoch(std::tm time)
{
   return std::mktime(&time);
}

bool datePartIsEqual(const std::tm& a, const std::tm& b)
{
   return a.tm_mday == b.tm_mday && a.tm_mon == b.tm_mon && a.tm_year == b.tm_year;
}

bool doesOverlap(const std::tm& startA, const std::tm& endA, const std::tm& startB, const std::tm& endB)
{
   return std::max(timeSinceEpoch(startA), timeSinceEpoch(startB)) < std::min(timeSinceEpoch(endA), timeSinceEpoch(endB));
}

struct ReportEntry
{
   int startLine{};
   int endLine{};
   std::tm startTime{};
   std::tm endTime{};
   bool isAllDay{};
   std::string category{};
   std::vector<std::string> comments{};
   std::vector<std::string> errors{};

   double duration() const
   {
      return timeToHours(std::difftime(timeSinceEpoch(endTime), timeSinceEpoch(startTime)));
   }

   void dump() const
   {
      std::cout << "startLine: " << startLine << "\n";
      std::cout << "endLine: " << endLine << "\n";
      std::cout << "startTime: " << std::put_time(&startTime, dateTimeFormat.c_str()) << "\n";
      std::cout << "endTime: " << std::put_time(&endTime, dateTimeFormat.c_str()) << "\n";
      std::cout << "duration: " << duration() << "\n";
      std::cout << "isAllDay: " << isAllDay << "\n";
      std::cout << "category: " << category << std::endl;
   }
};

static void ltrim(std::string& s)
{
   s.erase(s.begin(), std::find_if(s.begin(), s.end(), std::not1(std::ptr_fun<int, int>(std::isspace))));
}

static void rtrim(std::string& s)
{
   s.erase(std::find_if(s.rbegin(), s.rend(), std::not1(std::ptr_fun<int, int>(std::isspace))).base(), s.end());
}

static inline void trim(std::string& s)
{
   ltrim(s);
   rtrim(s);
}

std::vector<ReportEntry> parseFile(const std::string& fileName)
{
   std::ifstream file{};

   file.exceptions(std::ifstream::failbit | std::ifstream::badbit);
   file.open(fileName);
   file.exceptions(std::ifstream::goodbit);

   auto result = std::vector<ReportEntry>();
   int lineCounter = 1;
   while(!file.eof())
   {
      ReportEntry currentEntry{};

      /* find a line that starts with a date */
      for(std::string line{}; std::getline(file, line);)
      {
         lineCounter++;
         std::istringstream ss(line);
         ss >> std::skipws;
         bool hasTime = static_cast<bool>(ss >> std::get_time(&currentEntry.startTime, timeFormat.c_str()));
         if(!hasTime)
         {
            ss.clear();
            ss.seekg(0);
         }
         bool hasDate = static_cast<bool>(ss >> std::get_time(&currentEntry.startTime, dateFormat.c_str()));
         if(!hasDate)
         {
            continue;
         }

         currentEntry.startLine = lineCounter;
         std::getline(ss, currentEntry.category);
         trim(currentEntry.category);
         if(hasTime)
         {
            break;
         }

         currentEntry.isAllDay = true;
         currentEntry.endTime = currentEntry.startTime;
         currentEntry.endLine = currentEntry.startLine;

         result.push_back(currentEntry);

         currentEntry = ReportEntry();
      }

      /* if a start time was found then continue to search for an end time */
      if(currentEntry.startLine != 0)
      {
         for(std::string line{}; std::getline(file, line);)
         {
            lineCounter++;
            std::istringstream ss(line);
            bool gotEndTime = static_cast<bool>(ss >> std::get_time(&currentEntry.endTime, dateTimeFormat.c_str()));

            if(gotEndTime)
            {
               currentEntry.endLine = lineCounter;
               result.push_back(currentEntry);

               ReportEntry newEntry{};
               std::getline(ss, newEntry.category);
               trim(newEntry.category);
               newEntry.startLine = currentEntry.endLine;
               newEntry.startTime = currentEntry.endTime;
               currentEntry = newEntry;
            }
            else
            {
               if(currentEntry.category.empty() && line.find(entrySeparator) != std::string::npos)
               {
                  break;
               }
               if(std::string::npos != line.find_first_not_of(' '))
               {
                  currentEntry.comments.push_back(line);
               }
            }
         }
      }
   }
   return result;
}

void detectErrors(std::vector<ReportEntry>& entries)
{
   if(entries.size() == 0)
   {
      return;
   }
   double lastEnd = timeSinceEpoch(entries[0].startTime);
   double durationWithoutBreak = 0;
   for(auto& entry : entries)
   {
      auto start = timeSinceEpoch(entry.startTime);
      auto end = timeSinceEpoch(entry.endTime);

      if(timeToHours(start - lastEnd) < 0.5)
      {
         durationWithoutBreak += entry.duration();
      }
      else
      {
         durationWithoutBreak = entry.duration();
      }

      if(durationWithoutBreak > 9.0)
      {
         entry.errors.push_back("You should take a break for at least half an hour every 9 hours.");
      }

      if(!datePartIsEqual(entry.startTime, entry.endTime))
      {
         entry.errors.push_back("Start and end time should be on the same day.");
      }

      if(entry.duration() > 9)
      {
         entry.errors.push_back("Duration is longer than 9 hours.");
      }

      if(entry.category.empty())
      {
         entry.errors.push_back("Category was left empty.");
      }

      if(start > end)
      {
         entry.errors.push_back("The end time is prior the start time.");
      }

      if(start < lastEnd)
      {
         entry.errors.push_back("This entry is out of chronological order.");
      }

      for(const auto& comment : entry.comments)
      {
         if(comment.find(entrySeparator) != std::string::npos)
         {
            entry.errors.push_back("An entry should not contain a separator.");
            break;
         }
      }

      lastEnd = end;
   }
}

void printRow(std::ostream& output, const std::tm& date, const double duration, const std::vector<std::string>& comments)
{
   output << R"XML(        <Row  ss:AutoFitHeight="1">
)XML";
   output << R"XML(         <Cell ss:StyleID="date"><Data ss:Type="DateTime">)XML" << std::put_time(&date, "%Y-%m-%d")
          << R"XML(T00:00:00.000</Data></Cell>
)XML";

   double hours;
   double minutes = modf(duration, &hours) * 60.0;


   double roundedMinutes = round(minutes / 15.0) * 15.0;
   double roundedHours = hours;

   if(60.0 == roundedMinutes)
   {
      roundedMinutes = 0;
      roundedHours++;
   }


   output << R"XML(         <Cell ss:StyleID="time"><Data ss:Type="DateTime">1899-12-31T)XML" << std::setfill('0') << std::setw(2)
          << roundedHours << ":" << std::setfill('0') << std::setw(2) << roundedMinutes << R"XML(:00.000</Data></Cell>
)XML";
   output << R"XML(         <Cell ss:StyleID="decimal" ss:Formula="=R[0]C[-1] * 24"/>
)XML";
   output << R"XML(         <Cell><Data ss:Type="String">)XML";

   bool first = true;
   for(const auto& commet : comments)
   {
      if(first)
      {
         first = false;
      }
      else
      {
         output << "; "; //"&#10;";
      }
      output << commet;
   }
   output << "</Data></Cell>\n";

   output << R"XML(         <Cell ss:StyleID="time"><Data ss:Type="DateTime">1899-12-31T)XML" << std::setfill('0') << std::setw(2) << hours
          << ":" << std::setfill('0') << std::setw(2) << minutes << R"XML(:00.000</Data></Cell>
)XML";

   output << "        </Row>\n";
}

void printRow(std::ostream& output, const std::tm& date)
{
   std::vector<std::string> comments{};
   printRow(output, date, 0, comments);
}

void printReportCategories(std::vector<ReportEntry> entries)
{
   std::map<std::string, double> categoryMap;
   for(const auto& entry : entries)
   {
      categoryMap[entry.category] += entry.duration();
   }
   auto defaultPrecision = std::cout.precision();
   std::cout << std::setprecision(0) << std::fixed;
   for(const auto& kvp : categoryMap)
   {
      static const double workDaysPerWeek = 5.0;
      static const double workHoursPerDay = 8.0;
      double weeks;
      double days;
      double hours;
      double minutes = modf(modf(modf(kvp.second / workHoursPerDay / workDaysPerWeek, &weeks) * workDaysPerWeek, &days) * workHoursPerDay, &hours) * 60.0;
      std::cout << kvp.first << ": " << weeks << "w " << days << "d " << hours << "h " << minutes << "m (" << std::setprecision(2) << kvp.second
                << std::setprecision(0) << "h)\n";
   }
   std::cout.precision(defaultPrecision);
}

void printReport(std::ostream& output, std::vector<ReportEntry> entries)
{
   printReportCategories(entries);

   for(const auto& entry : entries)
   {
      if(entry.errors.size() == 0)
      {
         continue;
      }

      std::cerr << "[" << entry.startLine << "]\n";
      for(const auto& error : entry.errors)
      {
         std::cerr << "  " << error << "\n";
      }
   }

   output << R"XML(<?xml version="1.0"?>
<Workbook xmlns="urn:schemas-microsoft-com:office:spreadsheet"
 xmlns:o="urn:schemas-microsoft-com:office:office"
 xmlns:x="urn:schemas-microsoft-com:office:excel"
 xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
 xmlns:html="http://www.w3.org/TR/REC-html40">
  <Styles>
    <Style ss:ID="bold">
      <Font ss:Bold="1"/>
    </Style>
    <Style ss:ID="wrap">
      <Alignment ss:Vertical="Center" ss:Horizontal="Left" ss:WrapText="1"/>
      <Borders>
        <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="2"/>
        <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="2"/>
      </Borders>
      </Style>
    <Style ss:ID="decimal">
      <NumberFormat ss:Format="Fixed"/>
      <Alignment ss:Vertical="Center" ss:Horizontal="Center" />
      <Borders>
        <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="2"/>
        <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="2"/>
      </Borders>
    </Style>
    <Style ss:ID="time">
      <NumberFormat ss:Format="Short Time"/>
      <Alignment ss:Vertical="Center" ss:Horizontal="Center" />
      <Borders>
        <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="2"/>
        <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="2"/>
      </Borders>
    </Style>
    <Style ss:ID="date">
      <NumberFormat ss:Format="Long Date"/>
      <Alignment ss:Vertical="Center" ss:Horizontal="Center" />
      <Borders>
        <Border ss:Position="Right" ss:LineStyle="Continuous" ss:Weight="2"/>
        <Border ss:Position="Left" ss:LineStyle="Continuous" ss:Weight="2"/>
      </Borders>
    </Style>
  </Styles>
  <Worksheet ss:Name="Report">
    <Table>
      <ss:Column ss:Width="80"/>
      <ss:Column ss:Width="80"/>
      <ss:Column ss:Width="80"/>
      <ss:Column ss:Width="400"/>
      <Row ss:StyleID="bold">
        <Cell><Data ss:Type="String">Date</Data></Cell>
        <Cell><Data ss:Type="String">Time</Data></Cell>
        <Cell><Data ss:Type="String">Hours</Data></Cell>
        <Cell><Data ss:Type="String">Comments</Data></Cell>
        <Cell><Data ss:Type="String">Time</Data></Cell>
      </Row>)XML";

   auto it = entries.begin();
   if(it != entries.end())
   {
      std::tm currentDay{it->startTime};
      currentDay.tm_hour = currentDay.tm_min = currentDay.tm_sec = 0;

      while(it != entries.end())
      {
         std::tm startTime{it->startTime};
         std::tm lastStartTime{it->startTime};
         std::tm endTime{it->endTime};
         std::vector<std::string> comments{it->comments};
         double duration{it->duration()};
         bool isAllDay = it->isAllDay;
         it++;

         if(isAllDay)
         {
            continue;
         }
         for(; it != entries.end(); ++it)
         {
            if(!datePartIsEqual(lastStartTime, it->startTime))
            {
               break;
            }

            comments.insert(std::end(comments), std::begin(it->comments), std::end(it->comments));
            duration += it->duration();
            lastStartTime = it->startTime;
            endTime = it->startTime;
         }
         /* Do not dereference the itterator because it could be at the end already. */

         if(timeSinceEpoch(currentDay) <= timeSinceEpoch(startTime))
         {
            while(!datePartIsEqual(currentDay, startTime))
            {
               printRow(output, currentDay);
               currentDay.tm_mday++;
               std::mktime(&currentDay);
            }
         }

         currentDay = startTime;
         currentDay.tm_hour = currentDay.tm_min = currentDay.tm_sec = 0;
         currentDay.tm_mday++;
         std::mktime(&currentDay);

         printRow(output, startTime, duration, comments);
      }
   }

   output << R"XML(
  </Table>
 </Worksheet>
</Workbook>
)XML";
}

int main(int argc, char* argv[])
{
   if(argc < 3)
   {
      std::cerr << "Too few arguments.";
      return 1;
   }

   auto entries = parseFile(argv[1]);
   detectErrors(entries);
   std::ofstream output{argv[2]};
   printReport(output, entries);
   return 0;
}
